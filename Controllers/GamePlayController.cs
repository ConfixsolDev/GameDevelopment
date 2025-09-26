using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Services;

namespace TechWebSol.Controllers
{
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
    }
}
