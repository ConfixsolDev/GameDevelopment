using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Services;
using TechWebSol.Models;

namespace TechWebSol.Controllers
{
    public class UpdateTokenPositionRequest
    {
        public Guid TokenId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
    [AuthorizeDynamic]
    public class GamePlayController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<GamePlayController> _logger;

        public GamePlayController(
            ApplicationDbContext context,
            IUserSessionService userSessionService,
            ILogger<GamePlayController> logger)
        {
            _context = context;
            _userSessionService = userSessionService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Game Play Arena";
            ViewData["Subtitle"] = "Strategic Command Center - Fox Land vs Blue Land";
            return View();
        }

        /// <summary>
        /// Get tokens for the current user's team
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTeamTokens()
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Get user details from database to get TeamId
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUser.ApplicationUserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Find the team by TeamCode and SubTeamCode
                var team = await _context.Teams.FirstOrDefaultAsync(t => t.TeamId == user.TeamId);
                if (team == null)
                {
                    return Json(new { success = false, message = "Team not found" });
                }

                // Get tokens for this team
                var tokens = await _context.Tokens
                    .Include(t => t.TokenGroup)
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Name)
                    .Select(t => new
                    {
                        id = t.Id,
                        name = t.Name,
                        category = t.Category,
                        description = t.Description,
                        tokenGroupId = t.TokenGroupId,
                        tokenGroupName = t.TokenGroup != null ? t.TokenGroup.Name : null,
                        isActive = t.IsActive,
                        isManualToken = t.IsManualToken,
                        lastUsed = t.LastUsed,
                        usageCount = t.UsageCount,
                        notes = t.Notes,
                        createdAt = t.CreatedDate ?? DateTime.Now
                    })
                    .ToListAsync();

                return Json(new { success = true, tokens = tokens });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team tokens");
                return Json(new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update token position on the map
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateTokenPosition([FromBody] UpdateTokenPositionRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Find or create a MapMarker for this token
                var mapMarker = await _context.MapMarkers
                    .FirstOrDefaultAsync(m => m.TokenId == request.TokenId);

                if (mapMarker == null)
                {
                    // Create new MapMarker
                    var tokenEntity = await _context.Tokens.FindAsync(request.TokenId);
                    if (tokenEntity == null)
                    {
                        return Json(new { success = false, message = "Token not found" });
                    }

                    mapMarker = new MapMarker
                    {
                        Id = $"token_{request.TokenId}_{DateTime.Now.Ticks}",
                        TokenId = request.TokenId,
                        Location = $"{{\"lat\":{request.Latitude},\"lng\":{request.Longitude}}}",
                        CreatedAt = DateTime.Now,
                        TokenName = tokenEntity.Name,
                        CreatedBy = currentUser.ApplicationUserId.ToString(),
                        IsActive = true,
                        LastUpdated = DateTime.Now
                    };

                    _context.MapMarkers.Add(mapMarker);
                }
                else
                {
                    // Update existing MapMarker
                    mapMarker.Location = $"{{\"lat\":{request.Latitude},\"lng\":{request.Longitude}}}";
                    mapMarker.LastUpdated = DateTime.Now;
                }

                // Update token usage statistics
                var tokenForStats = await _context.Tokens.FindAsync(request.TokenId);
                if (tokenForStats != null)
                {
                    tokenForStats.LastUsed = DateTime.Now;
                    tokenForStats.UsageCount = tokenForStats.UsageCount + 1;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Token position updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating token position");
                return Json(new { success = false, message = "Error updating token position" });
            }
        }

        /// <summary>
        /// Get token position from MapMarker
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTokenPosition(Guid tokenId)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var mapMarker = await _context.MapMarkers
                    .FirstOrDefaultAsync(m => m.TokenId == tokenId && m.IsActive);

                if (mapMarker == null)
                {
                    return Json(new { success = false, message = "Token position not found" });
                }

                // Parse the JSON location string
                try
                {
                    var locationData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, double>>(mapMarker.Location);
                    if (locationData != null && locationData.ContainsKey("lat") && locationData.ContainsKey("lng"))
                    {
                        var position = new { lat = locationData["lat"], lng = locationData["lng"] };
                        return Json(new { success = true, position = position });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing location JSON for token {TokenId}", tokenId);
                }

                return Json(new { success = false, message = "Invalid position data" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token position");
                return Json(new { success = false, message = "Error retrieving token position" });
            }
        }
    }
}
