using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;
using TechWebSol.ViewModels;

namespace TechWebSol.Controllers
{
    [AuthorizeDynamic]
    public class SuspectedTokenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<SuspectedTokenController> _logger;
        private readonly ApplicationUserVM user;

        public SuspectedTokenController(
            ApplicationDbContext context,
            IUserSessionService userSessionService,
            ILogger<SuspectedTokenController> logger)
        {
            _context = context;
            _userSessionService = userSessionService;
            _logger = logger;
            user = userSessionService.GetCurrentUser();
        }

        /// <summary>
        /// Get all suspected tokens for the current user's team
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSuspectedTokens()
        {
            try
            {
                var suspectedTokens = await _context.SuspectedTokens
                    .Where(st => st.TeamId == user.TeamId && st.IsActive)
                    .Include(st => st.ISRMissions)
                    .OrderByDescending(st => st.CreatedDate)
                    .Select(st => new
                    {
                        id = st.Id,
                        name = st.Name,
                        placerSide = st.PlacerSide,
                        position = new
                        {
                            lat = st.Latitude.ToString(),
                            lng = st.Longitude.ToString()
                        },
                        source = st.Source,
                        confidence = st.Confidence,
                        status = st.Status,
                        notes = st.Notes,
                        suspectedType = st.SuspectedType,
                        markerStyle = st.MarkerStyle,
                        firstDetectedAt = st.FirstDetectedAt,
                        lastConfirmedAt = st.LastConfirmedAt,
                        teamId = st.TeamId,
                        createdBy = st.CreatedBy,
                        createdDate = st.CreatedDate,
                        isrMissionsCount = st.ISRMissions != null ? st.ISRMissions.Count : 0
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    suspectedTokens = suspectedTokens
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suspected tokens for team {TeamId}", user.TeamId);
                return Json(new { success = false, message = "Error retrieving suspected tokens" });
            }
        }

        /// <summary>
        /// Place a suspected token on the map
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PlaceSuspectedToken([FromBody] PlaceSuspectedTokenRequest request)
        {
            try
            {
                // Get team force type
                var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == user.TeamId);
                if (team == null)
                {
                    return Json(new { success = false, message = "Team not found" });
                }

                var suspectedToken = new SuspectedToken
                {
                    Name = request.Name ?? $"Contact-{DateTime.Now:yyyyMMddHHmmss}",
                    PlacerSide = team.ForceType ?? "Unknown",
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Source = request.Source ?? "human",
                    Confidence = request.Confidence ?? 40,
                    Status = "suspected",
                    Notes = request.Notes,
                    SuspectedType = request.SuspectedType,
                    MarkerStyle = "question-mark",
                    FirstDetectedAt = DateTime.Now,
                    LastConfirmedAt = DateTime.Now,
                    TeamId = user.TeamId,
                    CreatedBy = user.ApplicationUserId,
                    IsActive = true
                };

                _context.SuspectedTokens.Add(suspectedToken);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Suspected contact placed successfully",
                    suspectedToken = new
                    {
                        id = suspectedToken.Id,
                        name = suspectedToken.Name,
                        placerSide = suspectedToken.PlacerSide,
                        position = new
                        {
                            lat = suspectedToken.Latitude.ToString(),
                            lng = suspectedToken.Longitude.ToString()
                        },
                        source = suspectedToken.Source,
                        confidence = suspectedToken.Confidence,
                        status = suspectedToken.Status,
                        markerStyle = suspectedToken.MarkerStyle
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing suspected token");
                return Json(new { success = false, message = "Error placing suspected token" });
            }
        }

        /// <summary>
        /// Update a suspected token's position or properties
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateSuspectedToken([FromBody] UpdateSuspectedTokenRequest request)
        {
            try
            {
                var suspectedToken = await _context.SuspectedTokens
                    .FirstOrDefaultAsync(st => st.Id == request.TokenId && st.TeamId == user.TeamId);

                if (suspectedToken == null)
                {
                    return Json(new { success = false, message = "Suspected token not found" });
                }

                if (request.Latitude.HasValue) suspectedToken.Latitude = request.Latitude.Value;
                if (request.Longitude.HasValue) suspectedToken.Longitude = request.Longitude.Value;
                if (!string.IsNullOrEmpty(request.Name)) suspectedToken.Name = request.Name;
                if (request.Confidence.HasValue) suspectedToken.Confidence = request.Confidence.Value;
                if (!string.IsNullOrEmpty(request.Status)) suspectedToken.Status = request.Status;
                if (!string.IsNullOrEmpty(request.Notes)) suspectedToken.Notes = request.Notes;
                if (!string.IsNullOrEmpty(request.SuspectedType)) suspectedToken.SuspectedType = request.SuspectedType;

                suspectedToken.LastConfirmedAt = DateTime.Now;
                suspectedToken.UpdatedBy = user.ApplicationUserId;
                suspectedToken.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Suspected token updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating suspected token {TokenId}", request.TokenId);
                return Json(new { success = false, message = "Error updating suspected token" });
            }
        }

        /// <summary>
        /// Remove a suspected token from the map
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RemoveSuspectedToken([FromBody] RemoveSuspectedTokenRequest request)
        {
            try
            {
                var suspectedToken = await _context.SuspectedTokens
                    .FirstOrDefaultAsync(st => st.Id == request.TokenId && st.TeamId == user.TeamId);

                if (suspectedToken == null)
                {
                    return Json(new { success = false, message = "Suspected token not found" });
                }

                suspectedToken.IsActive = false;
                suspectedToken.UpdatedBy = user.ApplicationUserId;
                suspectedToken.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Suspected token removed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing suspected token {TokenId}", request.TokenId);
                return Json(new { success = false, message = "Error removing suspected token" });
            }
        }

        /// <summary>
        /// Create an ISR mission for a suspected token
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateISRMission([FromBody] CreateISRMissionRequest request)
        {
            try
            {
                var suspectedToken = await _context.SuspectedTokens
                    .FirstOrDefaultAsync(st => st.Id == request.SuspectedTokenId && st.TeamId == user.TeamId);

                if (suspectedToken == null)
                {
                    return Json(new { success = false, message = "Suspected token not found" });
                }

                var isrMission = new ISRMission
                {
                    Name = request.Name ?? $"ISR-{DateTime.Now:yyyyMMddHHmmss}",
                    SuspectedTokenId = request.SuspectedTokenId,
                    AssetType = request.AssetType ?? "uav",
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    ConfidenceGain = request.ConfidenceGain ?? 20,
                    CostFuel = request.CostFuel,
                    ExposureRisk = request.ExposureRisk,
                    RequestedBy = user.FullName,
                    Status = "scheduled",
                    TeamId = user.TeamId,
                    CreatedBy = user.ApplicationUserId,
                    IsActive = true
                };

                _context.ISRMissions.Add(isrMission);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "ISR mission created successfully",
                    mission = new
                    {
                        id = isrMission.Id,
                        name = isrMission.Name,
                        assetType = isrMission.AssetType,
                        status = isrMission.Status,
                        confidenceGain = isrMission.ConfidenceGain
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ISR mission");
                return Json(new { success = false, message = "Error creating ISR mission" });
            }
        }
    }

    // Request DTOs
    public class PlaceSuspectedTokenRequest
    {
        public string? Name { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Source { get; set; }
        public decimal? Confidence { get; set; }
        public string? Notes { get; set; }
        public string? SuspectedType { get; set; }
    }

    public class UpdateSuspectedTokenRequest
    {
        public Guid TokenId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Name { get; set; }
        public decimal? Confidence { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public string? SuspectedType { get; set; }
    }

    public class RemoveSuspectedTokenRequest
    {
        public Guid TokenId { get; set; }
    }

    public class CreateISRMissionRequest
    {
        public Guid SuspectedTokenId { get; set; }
        public string? Name { get; set; }
        public string? AssetType { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? ConfidenceGain { get; set; }
        public decimal? CostFuel { get; set; }
        public decimal? ExposureRisk { get; set; }
    }
}

