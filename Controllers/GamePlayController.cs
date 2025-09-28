using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;
using TechWebSol.Services.TokenManagement;
using TechWebSol.ViewModels;

namespace TechWebSol.Controllers
{
    [AuthorizeDynamic]
    public class GamePlayController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserSessionService _userSessionService;
        private readonly ITokenPlacementService _tokenPlacementService;
        private readonly ILogger<GamePlayController> _logger;
        private readonly ApplicationUserVM user;


        public GamePlayController(
            ApplicationDbContext context,
            IUserSessionService userSessionService,
            ITokenPlacementService tokenPlacementService,
            ILogger<GamePlayController> logger)
        {
            _context = context;
            _userSessionService = userSessionService;
            _tokenPlacementService = tokenPlacementService;
            user = userSessionService.GetCurrentUser();
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Game Play Arena";
            ViewData["Subtitle"] = "Strategic Command Center - Fox Land vs Blue Land";
            return View();
        }

        /// <summary>
        /// Get all placed tokens with their positions based on user's TeamId
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPlacedTokens()
        {
            try
            {
                var placedTokens = await _context.Tokens
                    .Where(t => t.TeamId == user.TeamId && t.IsActive)
                    .Where(t => t.MapMarkers.Any(m => m.IsActive))
                    .Include(t => t.MapMarkers.OrderByDescending(x=>x.CreatedDate))
                    .Include(t => t.TokenGroup)
                    .Include(t => t.AreaCoverages)
                    .Select(t => new
                    {
                        id = t.Id,
                        name = t.Name,
                        tokenGroupId = t.TokenGroupId,
                        tokenGroupName = t.TokenGroup != null ? t.TokenGroup.Name : null,
                        assetImagePath = t.AssetImagePath,
                        position = t.MapMarkers.Where(m => m.IsActive).Select(m => new { lat = m.latitude, lng = m.longitude }).FirstOrDefault(),
                        isActive = t.IsActive,
                        isManualToken = t.IsManualToken,
                        lastUsed = t.LastUsed,
                        usageCount = t.UsageCount,
                        notes = t.Notes,
                        status = "placed",
                        areaCoverages = t.AreaCoverages.Select(ac => new
                        {
                            id = ac.Id,
                            shapeType = ac.ShapeType,
                            radiusKm = ac.RadiusKm,
                            coverageType = ac.CoverageType,
                            geometry = ac.Geometry
                        }).ToList()
                    })
                    .ToListAsync();


                return Json(new { 
                    success = true, 
                    tokens = placedTokens 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting placed tokens for team {TeamId}", _userSessionService.GetCurrentUser()?.TeamId);
                return Json(new { success = false, message = "Error retrieving placed tokens" });
            }
        }

        /// <summary>
        /// Load partial view for lazy loading
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> LoadPartial(string partialName)
        {
            try
            {
                // Validate and sanitize partial name
                if (string.IsNullOrWhiteSpace(partialName))
                {
                    return BadRequest("Partial name is required");
                }

                // Define allowed partials for security
                var allowedPartials = new Dictionary<string, string>
                {
                    { "region-panel", "Partials/Controls/_RegionPanel" },
                    { "overlay-controls", "Partials/Controls/_OverlayControls" },
                    { "data-entry-modal", "Partials/Modals/_DataEntryModal" },
                    { "token-management-modal", "Partials/Modals/_TokenManagementModal" },
                    { "token-selection-modal", "Partials/Modals/_TokenSelectionModal" },
                    { "token-brigade-data-modal", "_TokenBrigadeData" },
                    { "simulation-panel", "Partials/Modals/_SimulationPanel" },
                    { "unit-deployment-modal", "Partials/Modals/_UnitDeploymentModal" },
                    { "movement-plan-modal", "Partials/Modals/_MovementPlanModal" },
                    { "battle-modal", "Partials/Modals/_BattleModal" },
                    { "objective-modal", "Partials/Modals/_ObjectiveModal" },
                    { "settings-modal", "Partials/Modals/_SettingsModal" },
                    { "scripts-core", "Partials/Scripts/_CoreScripts" },
                    { "scripts-token", "Partials/Scripts/_TokenScripts" },
                    { "scripts-map", "Partials/Scripts/_MapScripts" },
                };

                if (!allowedPartials.ContainsKey(partialName))
                {
                    return NotFound($"Partial '{partialName}' not found");
                }

                var partialPath = allowedPartials[partialName];

                // Return the partial view
                return PartialView(partialPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading partial view: {PartialName}", partialName);
                return StatusCode(500, "Error loading partial view");
            }
        }

        /// <summary>
        /// Get tokens for the current user's team
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTeamTokens()
        {
            try
            {
                var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == user.TeamId);
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
                        tokenGroupId = t.TokenGroupId,
                        tokenGroupName = t.TokenGroup != null ? t.TokenGroup.Name : null,
                        assetImagePath = t.AssetImagePath,
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
        /// Place token on the map with coverage area creation
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PlaceTokenOnMap([FromBody] PlaceTokenRequest request)
        {
            try
            {
                var result = await _tokenPlacementService.PlaceTokenOnMapAsync(
                    request.TokenId, 
                    request.Latitude, 
                    request.Longitude,
                    user.ApplicationUserId);

                if (result.Success)
                {
                    return Json(new 
                    { 
                        success = true, 
                        message = result.Message,
                        token = new
                        {
                            id = result.Token?.Id,
                            name = result.Token?.Name,
                            coverageRadius = result.Token?.CoverageRadiusKm
                        },
                        areaCoverages = result.AreaCoverages?.Select(ac => new
                        {
                            id = ac.Id,
                            name = ac.Name,
                            geometry = ac.Geometry,
                            coverageType = ac.CoverageType,
                            shapeType = ac.ShapeType,
                            radiusKm = ac.RadiusKm,
                            areaKm2 = ac.AreaKm2
                        })
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing token on map");
                return Json(new { success = false, message = "Error placing token on map" });
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
                var result = await _tokenPlacementService.UpdateTokenPositionAsync(
                    request.TokenId, 
                    (decimal)request.Latitude, 
                    (decimal)request.Longitude,
                    user.ApplicationUserId);

                if (result.Success)
                {
                    return Json(new 
                    { 
                        success = true, 
                        message = result.Message,
                        areaCoverages = result.AreaCoverages?.Select(ac => new
                        {
                            id = ac.Id,
                            name = ac.Name,
                            geometry = ac.Geometry,
                            coverageType = ac.CoverageType,
                            shapeType = ac.ShapeType,
                            radiusKm = ac.RadiusKm,
                            areaKm2 = ac.AreaKm2
                        })
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating token position");
                return Json(new { success = false, message = "Error updating token position" });
            }
        }

        /// <summary>
        /// Move token to new position
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MoveToken([FromBody] PlaceTokenRequest request)
        {
            try
            {

                var result = await _tokenPlacementService.UpdateTokenPositionAsync(
                    request.TokenId, 
                    request.Latitude, 
                    request.Longitude,
                    user.ApplicationUserId);

                if (result.Success)
                {
                    return Json(new 
                    { 
                        success = true, 
                        message = result.Message,
                        token = new
                        {
                            id = result.Token?.Id,
                            name = result.Token?.Name,
                            coverageRadius = result.Token?.CoverageRadiusKm
                        },
                        areaCoverages = result.AreaCoverages?.Select(ac => new
                        {
                            id = ac.Id,
                            name = ac.Name,
                            geometry = ac.Geometry,
                            coverageType = ac.CoverageType,
                            shapeType = ac.ShapeType,
                            radiusKm = ac.RadiusKm,
                            areaKm2 = ac.AreaKm2
                        })
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving token");
                return Json(new { success = false, message = "Error moving token" });
            }
        }

        /// <summary>
        /// Remove token from map permanently
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RemoveTokenFromMap([FromBody] RemoveTokenRequest request)
        {
            try
            {
                var result = await _tokenPlacementService.RemoveTokenFromMapAsync(
                    request.TokenId,
                    user.ApplicationUserId);

                if (result.Success)
                {
                    return Json(new 
                    { 
                        success = true, 
                        message = result.Message
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing token from map");
                return Json(new { success = false, message = "Error removing token from map" });
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
                var result = await _tokenPlacementService.GetTokenPlacementInfoAsync(tokenId);
                
                if (result.Success && result.MapMarker != null)
                {
                    // Parse the JSON location string
                    try
                    {
                        if (result.MapMarker != null)
                        {
                            var position = new { lat = result.MapMarker.latitude, lng = result.MapMarker.longitude };
                            return Json(new { 
                                success = true, 
                                position = position,
                                token = new
                                {
                                    id = result.Token?.Id,
                                    name = result.Token?.Name,
                                    coverageRadius = result.Token?.CoverageRadiusKm
                                },
                                areaCoverages = result.AreaCoverages?.Select(ac => new
                                {
                                    id = ac.Id,
                                    name = ac.Name,
                                    geometry = ac.Geometry,
                                    coverageType = ac.CoverageType,
                                    shapeType = ac.ShapeType,
                                    radiusKm = ac.RadiusKm,
                                    areaKm2 = ac.AreaKm2
                                })
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing location JSON for token {TokenId}", tokenId);
                    }
                }

                return Json(new { success = false, message = "Token position not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token position");
                return Json(new { success = false, message = "Error retrieving token position" });
            }
        }

        /// <summary>
        /// Returns the data entry token selection modal as a partial view
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DataEntryTokenSelection()
        {
            try
            {
                var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == user.TeamId);
                if (team == null)
                {
                    return PartialView("Partials/_ErrorPartial", new { Message = "Team not found" });
                }

                // Get tokens for this team
                var tokens = await _context.Tokens
                    .Include(t => t.TokenGroup)
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Name)
                    .ToListAsync();

                return PartialView("Partials/_DataEntryTokenSelection", tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data entry token selection");
                return PartialView("Partials/_ErrorPartial", new { Message = "Error loading tokens for data entry" });
            }
        }
    }

    public class PlaceTokenRequest
    {
        public Guid TokenId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }

    public class UpdateTokenPositionRequest
    {
        public Guid TokenId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime? PlacedAt { get; set; }
    }

    public class RemoveTokenRequest
    {
        public Guid TokenId { get; set; }
    }
}
