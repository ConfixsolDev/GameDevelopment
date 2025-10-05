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
                var placedTokensvar =  _context.Tokens
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
                        t.TeamId,
                        status = "placed",
                        forceType = t.ForceType,
                        areaCoverages = t.AreaCoverages.Select(ac => new
                        {
                            id = ac.Id,
                            shapeType = ac.ShapeType,
                            frontRadiusKm = ac.FrontRadiusKm,
                            rearRadiusKm = ac.RearRadiusKm,
                            sideRadiusKm = ac.SideRadiusKm,
                            rotationDegrees = ac.RotationDegrees,
                            coverageType = ac.CoverageType,
                            geometry = ac.Geometry
                        }).ToList(),
                        movementHistory = t.MapMarkers
                            .OrderBy(m => m.CreatedDate)
                            .Select(m => new
                            {
                                latitude = decimal.Parse(m.latitude),
                                longitude = decimal.Parse(m.longitude),
                                createdDate = m.CreatedDate,
                                isActive = m.IsActive,
                            }).ToList()
                    }).AsQueryable();

                if (user.TeamId != null)
                {
                    placedTokensvar = placedTokensvar.Where(t => t.TeamId == user.TeamId && t.isActive);
                }
                else
                {
                    placedTokensvar = placedTokensvar.Where(t => t.isActive);
                }

                 var  placedTokens = await placedTokensvar.ToListAsync();
                return Json(new
                    {
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
                    { "suspected-token-modal", "Partials/Modals/_SuspectedTokenModal" },
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
                    .Where(t => t.TeamId == user.TeamId && t.IsActive)
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
                            frontCoverageKm = result.Token?.FrontCoverageKm,
                            rearCoverageKm = result.Token?.RearCoverageKm,
                            sideCoverageKm = result.Token?.SideCoverageKm,
                            forceType = result.Token?.ForceType
                        },
                        areaCoverages = result.AreaCoverages?.Select(ac => new
                        {
                            id = ac.Id,
                            name = ac.Name,
                            geometry = ac.Geometry,
                            coverageType = ac.CoverageType,
                            shapeType = ac.ShapeType,
                            frontRadiusKm = ac.FrontRadiusKm,
                            rearRadiusKm = ac.RearRadiusKm,
                            sideRadiusKm = ac.SideRadiusKm,
                            rotationDegrees = ac.RotationDegrees,
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
                            frontRadiusKm = ac.FrontRadiusKm,
                            rearRadiusKm = ac.RearRadiusKm,
                            sideRadiusKm = ac.SideRadiusKm,
                            rotationDegrees = ac.RotationDegrees,
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
                // Update token position
                var result = await _tokenPlacementService.UpdateTokenPositionAsync(
                    request.TokenId, 
                    request.Latitude, 
                    request.Longitude,
                    user.ApplicationUserId);

                if (result.Success)
                {
                    // Save movement planning data if provided
                    if (!string.IsNullOrEmpty(request.MovementMode) || request.PlannedETA.HasValue)
                    {
                        await SaveMovementOrder(request, result.Token);
                    }

                    return Json(new 
                    { 
                        success = true, 
                        message = result.Message,
                        token = new
                        {
                            id = result.Token?.Id,
                            name = result.Token?.Name,
                            frontCoverageKm = result.Token?.FrontCoverageKm,
                            rearCoverageKm = result.Token?.RearCoverageKm,
                            sideCoverageKm = result.Token?.SideCoverageKm,
                            forceType = result.Token?.ForceType
                        },
                        areaCoverages = result.AreaCoverages?.Select(ac => new
                        {
                            id = ac.Id,
                            name = ac.Name,
                            geometry = ac.Geometry,
                            coverageType = ac.CoverageType,
                            shapeType = ac.ShapeType,
                            frontRadiusKm = ac.FrontRadiusKm,
                            rearRadiusKm = ac.RearRadiusKm,
                            sideRadiusKm = ac.SideRadiusKm,
                            rotationDegrees = ac.RotationDegrees,
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
        /// Save movement order with planning details
        /// </summary>
        private async Task SaveMovementOrder(PlaceTokenRequest request, Token? token)
        {
            if (token == null) return;

            try
            {
                // Get or create a scenario for this team
                var scenario = await _context.WarGameScenarios
                    .FirstOrDefaultAsync(s => s.TeamId == user.TeamId && s.Status == "Planning");

                if (scenario == null)
                {
                    scenario = new WarGameScenario
                    {
                        Id = Guid.NewGuid(),
                        Name = $"Scenario {user.TeamId}",
                        Description = "Auto-generated scenario for movement planning",
                        ScenarioCode = $"SCN_{user.TeamId}_{DateTime.UtcNow:yyyyMMdd}",
                        Status = "Planning",
                        TeamId = user.TeamId,
                        CreatedBy = user.FullName,
                        IsActive = true
                    };
                    _context.WarGameScenarios.Add(scenario);
                    await _context.SaveChangesAsync();
                }

                // Get or create unit deployment
                var deployment = await _context.UnitDeployments
                    .FirstOrDefaultAsync(d => d.TokenId == request.TokenId && d.ScenarioId == scenario.Id);

                if (deployment == null)
                {
                    deployment = new UnitDeployment
                    {
                        Id = Guid.NewGuid(),
                        ScenarioId = scenario.Id,
                        TokenId = request.TokenId,
                        UnitType = token.TokenGroup?.Name ?? "Unit",
                        UnitName = token.Name,
                        ForceType = "Blue", // Default, should be determined by team
                        Position = $"{{\"lat\": {request.Latitude}, \"lng\": {request.Longitude}}}",
                        TeamId = user.TeamId,
                        CreatedBy = user.FullName,
                        IsActive = true
                    };
                    _context.UnitDeployments.Add(deployment);
                    await _context.SaveChangesAsync();
                }

                // Create movement order
                var movementOrder = new MovementOrder
                {
                    Id = Guid.NewGuid(),
                    UnitDeploymentId = deployment.Id,
                    StartPosition = deployment.Position, // Current position
                    EndPosition = $"{{\"lat\": {request.Latitude}, \"lng\": {request.Longitude}}}",
                    MovementType = request.MovementMode ?? "Normal",
                    Status = "Planned",
                    Speed = request.MovementSpeed ?? 20,
                    Distance = CalculateDistance(deployment.Position, request.Latitude, request.Longitude),
                    EstimatedArrival = request.PlannedETA.HasValue ? 
                        DateTime.UtcNow.AddHours((double)request.PlannedETA.Value) : null,
                    StartTime = request.StartTurn.HasValue ? 
                        DateTime.UtcNow.AddHours(request.StartTurn.Value * 24 + (double)(request.StartOffset ?? 0)) : null,
                    EngagementRule = request.EngagementRule ?? "Avoid Strongpoints",
                    Notes = request.Notes,
                    TeamId = user.TeamId,
                    CreatedBy = user.FullName,
                    IsActive = true
                };

                _context.MovementOrders.Add(movementOrder);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Movement order saved for token {TokenId} with ETA {ETA}", 
                    request.TokenId, request.PlannedETA);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving movement order for token {TokenId}", request.TokenId);
            }
        }

        /// <summary>
        /// Calculate distance between two points
        /// </summary>
        private decimal CalculateDistance(string startPosition, decimal endLat, decimal endLng)
        {
            try
            {
                // Parse start position JSON
                var startPos = System.Text.Json.JsonSerializer.Deserialize<dynamic>(startPosition);
                var startLat = (decimal)startPos.GetProperty("lat").GetDecimal();
                var startLng = (decimal)startPos.GetProperty("lng").GetDecimal();

                // Calculate distance using Haversine formula
                const double R = 6371; // Earth's radius in kilometers
                var dLat = ToRadians((double)(endLat - startLat));
                var dLng = ToRadians((double)(endLng - startLng));
                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(ToRadians((double)startLat)) * Math.Cos(ToRadians((double)endLat)) *
                        Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                var distance = R * c;

                return (decimal)distance;
            }
            catch
            {
                return 0; // Return 0 if calculation fails
            }
        }

        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
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
        /// Get deployed units for movement planning
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDeployedUnits()
        {
            try
            {
                // Get placed tokens as deployed units
                var deployedUnits = await _context.Tokens
                    .Where(t => t.TeamId == user.TeamId && t.IsActive)
                    .Where(t => t.MapMarkers.Any(m => m.IsActive))
                    .Include(t => t.MapMarkers.OrderByDescending(x => x.CreatedDate))
                    .Include(t => t.TokenGroup)
                    .Select(t => new
                    {
                        id = t.Id,
                        name = t.Name,
                        type = t.TokenGroup != null ? t.TokenGroup.Name : "Unit",
                        strength = 100, // Default strength
                        position = t.MapMarkers.Where(m => m.IsActive).Select(m => new { lat = m.latitude, lng = m.longitude }).FirstOrDefault(),
                        status = "Deployed",
                        teamId = t.TeamId,
                        isActive = t.IsActive
                    })
                    .ToListAsync();

                return Json(new { 
                    success = true, 
                    units = deployedUnits 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deployed units for team {TeamId}", user.TeamId);
                return Json(new { success = false, message = "Error retrieving deployed units" });
            }
        }

        /// <summary>
        /// Get historical movement data for a token
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTokenMovementHistory(Guid tokenId)
        {
            try
            {
                var token = await _context.Tokens
                    .Include(t => t.TokenGroup)
                    .FirstOrDefaultAsync(t => t.Id == tokenId && t.TeamId == user.TeamId);

                if (token == null)
                {
                    return Json(new { success = false, message = "Token not found" });
                }

                // Get all historical positions (including inactive ones)
                var movementHistory = await _context.MapMarkers
                    .Where(m => m.TokenId == tokenId)
                    .OrderBy(m => m.CreatedDate)
                    .Select(m => new
                    {
                        id = m.Id,
                        latitude = decimal.Parse(m.latitude),
                        longitude = decimal.Parse(m.longitude),
                        createdDate = m.CreatedDate,
                        isActive = m.IsActive,
                        createdBy = m.CreatedBy
                    })
                    .ToListAsync();

                // Get movement orders for additional context
                var movementOrders = await _context.MovementOrders
                    .Where(mo => mo.TeamId == user.TeamId && mo.IsActive)
                    .Where(mo => mo.UnitDeployment != null && mo.UnitDeployment.TokenId == tokenId)
                    .OrderBy(mo => mo.CreatedDate)
                    .Select(mo => new
                    {
                        id = mo.Id,
                        movementType = mo.MovementType,
                        status = mo.Status,
                        speed = mo.Speed,
                        distance = mo.Distance,
                        estimatedArrival = mo.EstimatedArrival,
                        startTime = mo.StartTime,
                        notes = mo.Notes,
                        engagementRule = mo.EngagementRule,
                        createdDate = mo.CreatedDate
                    })
                    .ToListAsync();

                return Json(new { 
                    success = true, 
                    token = new
                    {
                        id = token.Id,
                        name = token.Name,
                        type = token.TokenGroup?.Name ?? "Unit"
                    },
                    movementHistory = movementHistory,
                    movementOrders = movementOrders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movement history for token {TokenId}", tokenId);
                return Json(new { success = false, message = "Error retrieving movement history" });
            }
        }

        /// <summary>
        /// Get all movement history for the team (for replay functionality)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTeamMovementHistory()
        {
            try
            {
                // Get all tokens with their movement history
                var tokensWithHistory = await _context.Tokens
                    .Where(t => t.TeamId == user.TeamId && t.IsActive)
                    .Include(t => t.TokenGroup)
                    .Include(t => t.MapMarkers.OrderBy(m => m.CreatedDate))
                    .Select(t => new
                    {
                        token = new
                        {
                            id = t.Id,
                            name = t.Name,
                            type = t.TokenGroup != null ? t.TokenGroup.Name : "Unit",
                            currentPosition = t.MapMarkers.Where(m => m.IsActive).Select(m => new { lat = decimal.Parse(m.latitude), lng = decimal.Parse(m.longitude) }).FirstOrDefault()
                        },
                        movementHistory = t.MapMarkers.Select(m => new
                        {
                            id = m.Id,
                            latitude = decimal.Parse(m.latitude),
                            longitude = decimal.Parse(m.longitude),
                            createdDate = m.CreatedDate,
                            isActive = m.IsActive,
                            createdBy = m.CreatedBy
                        }).OrderBy(m => m.createdDate).ToList()
                    })
                    .Where(t => t.movementHistory.Any())
                    .ToListAsync();

                // Get all movement orders for context
                var allMovementOrders = await _context.MovementOrders
                    .Where(mo => mo.TeamId == user.TeamId && mo.IsActive)
                    .Include(mo => mo.UnitDeployment)
                    .OrderBy(mo => mo.CreatedDate)
                    .Select(mo => new
                    {
                        id = mo.Id,
                        tokenId = mo.UnitDeployment.TokenId,
                        movementType = mo.MovementType,
                        status = mo.Status,
                        speed = mo.Speed,
                        distance = mo.Distance,
                        estimatedArrival = mo.EstimatedArrival,
                        startTime = mo.StartTime,
                        notes = mo.Notes,
                        engagementRule = mo.EngagementRule,
                        createdDate = mo.CreatedDate
                    })
                    .ToListAsync();

                return Json(new { 
                    success = true, 
                    tokens = tokensWithHistory,
                    movementOrders = allMovementOrders,
                    totalMovements = tokensWithHistory.Sum(t => t.movementHistory.Count)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team movement history for team {TeamId}", user.TeamId);
                return Json(new { success = false, message = "Error retrieving team movement history" });
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
                                    coverageRadius = result.Token?.FrontCoverageKm
                                },
                                areaCoverages = result.AreaCoverages?.Select(ac => new
                                {
                                    id = ac.Id,
                                    name = ac.Name,
                                    geometry = ac.Geometry,
                                    coverageType = ac.CoverageType,
                                    shapeType = ac.ShapeType,
                                    frontRadiusKm = ac.FrontRadiusKm,
                            rearRadiusKm = ac.RearRadiusKm,
                            sideRadiusKm = ac.SideRadiusKm,
                            rotationDegrees = ac.RotationDegrees,
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
        
        // Enhanced movement planning fields
        public string? MovementMode { get; set; }
        public int? StartTurn { get; set; }
        public decimal? StartOffset { get; set; }
        public decimal? PlannedETA { get; set; }
        public decimal? MovementSpeed { get; set; }
        public string? EngagementRule { get; set; }
        public bool? SharedOrder { get; set; }
        public string? Notes { get; set; }
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
