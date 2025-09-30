using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechWebSol.Data;
using TechWebSol.Models;
using TechWebSol.Services;

namespace TechWebSol.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovementController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly MovementService _movementService;
        private readonly IUserSessionService _userSessionService;

        public MovementController(ApplicationDbContext context, MovementService movementService, IUserSessionService userSessionService)
        {
            _context = context;
            _movementService = movementService;
            _userSessionService = userSessionService;
        }

        /// <summary>
        /// Get unit movement data for Phase 01 planning
        /// </summary>
        [HttpGet("unit/{unitId}")]
        public async Task<ActionResult<UnitMovementData>> GetUnitMovementData(Guid unitId)
        {
            try
            {
                var user = _userSessionService.GetCurrentUser();
                if (user == null)
                    return Unauthorized("User not authenticated");

                // Find the unit deployment
                var unitDeployment = await _context.UnitDeployments
                    .FirstOrDefaultAsync(ud => ud.UnitId == unitId && ud.TeamId == user.TeamId);

                if (unitDeployment == null)
                    return NotFound("Unit not found");

                var movementData = new UnitMovementData
                {
                    UnitId = unitId,
                    UnitName = unitDeployment.UnitName ?? "Unknown Unit",
                    UnitType = unitDeployment.UnitType ?? "Unknown",
                    MovementPointsPerTurn = unitDeployment.MovementPointsPerTurn,
                    RemainingMovementPoints = unitDeployment.RemainingMovementPoints,
                    CurrentTerrain = unitDeployment.CurrentTerrain ?? "OPEN",
                    SupplyState = unitDeployment.SupplyState ?? "Green",
                    CombatPowerIndex = unitDeployment.CombatPowerIndex,
                    EffectiveCombatPower = unitDeployment.EffectiveCombatPower_RO,
                    Position = new Position
                    {
                        Lat = 0, // Will be extracted from Position JSON if needed
                        Lng = 0
                    }
                };

                return Ok(movementData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving unit movement data: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculate movement cost for a route
        /// </summary>
        [HttpPost("calculate-cost")]
        public async Task<ActionResult<MovementCostResult>> CalculateMovementCost([FromBody] MovementCostRequest request)
        {
            try
            {
                if (request.Waypoints == null || request.Waypoints.Count < 2)
                    return BadRequest("At least 2 waypoints required");

                var totalDistance = 0.0;
                var totalCost = 0.0;

                // Calculate distance and cost for each segment
                for (int i = 0; i < request.Waypoints.Count - 1; i++)
                {
                    var from = request.Waypoints[i];
                    var to = request.Waypoints[i + 1];

                    // Calculate distance (simplified - in real implementation, use proper distance calculation)
                    var segmentDistance = CalculateDistance(from.Lat, from.Lng, to.Lat, to.Lng);
                    totalDistance += segmentDistance;

                    // Calculate movement cost for this segment
                    var terrainType = request.TerrainType ?? "OPEN";
                    var movementType = request.MovementType ?? "road";
                    var segmentCost = await _movementService.CalculateMovementCost(terrainType, movementType);
                    
                    totalCost += segmentDistance * segmentCost;
                }

                var result = new MovementCostResult
                {
                    TotalDistanceKm = Math.Round(totalDistance, 2),
                    TotalMovementCost = Math.Round(totalCost, 1),
                    EstimatedTimeHours = Math.Round(totalDistance / 10.0, 1), // Assume 10 km/h average
                    SupplyImpact = GetSupplyImpact(totalCost),
                    IsValid = true
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error calculating movement cost: {ex.Message}");
            }
        }

        /// <summary>
        /// Validate if unit can perform movement
        /// </summary>
        [HttpPost("validate-movement")]
        public async Task<ActionResult<MovementValidationResult>> ValidateMovement([FromBody] MovementValidationRequest request)
        {
            try
            {
                var user = _userSessionService.GetCurrentUser();
                if (user == null)
                    return Unauthorized("User not authenticated");

                var unitDeployment = await _context.UnitDeployments
                    .FirstOrDefaultAsync(ud => ud.UnitId == request.UnitId && ud.TeamId == user.TeamId);

                if (unitDeployment == null)
                    return NotFound("Unit not found");

                var availableMovement = await _movementService.GetEffectiveMovement(unitDeployment);
                var canMove = request.RequiredMovementCost <= availableMovement;

                var result = new MovementValidationResult
                {
                    UnitId = request.UnitId,
                    AvailableMovement = availableMovement,
                    RequiredMovement = request.RequiredMovementCost,
                    CanMove = canMove,
                    RemainingAfterMove = canMove ? availableMovement - request.RequiredMovementCost : 0,
                    Warnings = new List<string>()
                };

                // Add warnings
                if (!canMove)
                {
                    result.Warnings.Add($"Insufficient movement points. Need {request.RequiredMovementCost}, have {availableMovement}");
                }
                else if (result.RemainingAfterMove < 5)
                {
                    result.Warnings.Add($"Warning: Low remaining movement points ({result.RemainingAfterMove})");
                }

                if (unitDeployment.SupplyState == "Red")
                {
                    result.Warnings.Add("Unit has Red supply state - movement effectiveness reduced");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error validating movement: {ex.Message}");
            }
        }

        /// <summary>
        /// Save a route draft
        /// </summary>
        [HttpPost("save-draft")]
        public async Task<ActionResult<RouteDraftResult>> SaveRouteDraft([FromBody] RouteDraftRequest request)
        {
            try
            {
                var user = _userSessionService.GetCurrentUser();
                if (user == null)
                    return Unauthorized("User not authenticated");

                // Validate unit exists
                var unitDeployment = await _context.UnitDeployments
                    .FirstOrDefaultAsync(ud => ud.UnitId == request.UnitId && ud.TeamId == user.TeamId);

                if (unitDeployment == null)
                    return NotFound("Unit not found");

                // Create route draft
                var routeDraft = new RoutesDraft
                {
                    UnitId = unitDeployment.Id,
                    RouteName = request.RouteName ?? $"Route {DateTime.Now:yyyy-MM-dd HH:mm}",
                    WaypointsJson = System.Text.Json.JsonSerializer.Serialize(request.Waypoints),
                    TotalDistanceKm = (decimal)request.TotalDistanceKm,
                    EstimatedTimeTurns = (int)request.EstimatedTimeHours,
                    SupplyImpact = (decimal)(request.SupplyImpact ?? 0),
                    IsCommitted = false,
                    TeamId = user.TeamId,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.RoutesDrafts.Add(routeDraft);
                await _context.SaveChangesAsync();

                var result = new RouteDraftResult
                {
                    RouteId = routeDraft.Id,
                    RouteName = routeDraft.RouteName,
                    TotalDistanceKm = (double)routeDraft.TotalDistanceKm,
                    ETA = routeDraft.CreatedDate?.AddHours(routeDraft.EstimatedTimeTurns),
                    IsSaved = true
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving route draft: {ex.Message}");
            }
        }

        /// <summary>
        /// Commit a movement or assembly action
        /// </summary>
        [HttpPost("commit-action")]
        public async Task<ActionResult<MovementCommitResult>> CommitMovement([FromBody] MovementCommitRequest request)
        {
            try
            {
                var user = _userSessionService.GetCurrentUser();
                if (user == null)
                    return Unauthorized("User not authenticated");

                var unitDeployment = await _context.UnitDeployments
                    .FirstOrDefaultAsync(ud => ud.UnitId == request.UnitId && ud.TeamId == user.TeamId);

                if (unitDeployment == null)
                    return NotFound("Unit not found");

                // Validate movement
                var availableMovement = await _movementService.GetEffectiveMovement(unitDeployment);
                if (request.MovementCost > availableMovement)
                {
                    return BadRequest($"Insufficient movement points. Need {request.MovementCost}, have {availableMovement}");
                }

                // Update unit deployment
                unitDeployment.RemainingMovementPoints -= (int)request.MovementCost;
                
                if (request.ActionType == "assembly")
                {
                    // For assembly, we could add a status field or notes
                    unitDeployment.Status = "Assembled";
                }

                // Update position if waypoints provided
                if (request.Waypoints != null && request.Waypoints.Count > 0)
                {
                    var finalWaypoint = request.Waypoints.Last();
                    // Update position JSON
                    unitDeployment.Position = System.Text.Json.JsonSerializer.Serialize(new { 
                        lat = finalWaypoint.Lat, 
                        lng = finalWaypoint.Lng 
                    });
                    unitDeployment.CurrentTerrain = request.TerrainType ?? unitDeployment.CurrentTerrain;
                }

                // Update effective combat power
                unitDeployment.UpdateEffectiveCombatPower();

                await _context.SaveChangesAsync();

                var result = new MovementCommitResult
                {
                    UnitId = request.UnitId,
                    ActionType = request.ActionType,
                    MovementCost = request.MovementCost,
                    RemainingMovement = unitDeployment.RemainingMovementPoints,
                    IsCommitted = true,
                    NewPosition = request.Waypoints?.LastOrDefault(),
                    EffectiveCombatPower = unitDeployment.EffectiveCombatPower_RO
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error committing movement: {ex.Message}");
            }
        }

        /// <summary>
        /// Get terrain types for selection
        /// </summary>
        [HttpGet("terrain-types")]
        public async Task<ActionResult<List<TerrainType>>> GetTerrainTypes()
        {
            try
            {
                var user = _userSessionService.GetCurrentUser();
                if (user == null)
                    return Unauthorized("User not authenticated");

                var terrainTypes = await _context.TerrainTypes
                    .Where(t => t.TeamId == user.TeamId || t.TeamId == null)
                    .ToListAsync();

                if (!terrainTypes.Any())
                {
                    // Return default terrain types if none exist
                    terrainTypes = await _movementService.GetDefaultTerrainTypesAsync();
                }

                return Ok(terrainTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving terrain types: {ex.Message}");
            }
        }

        /// <summary>
        /// Health check endpoint for Phase 01 readiness
        /// </summary>
        [HttpGet("health")]
        public async Task<ActionResult<MovementHealthCheck>> HealthCheck()
        {
            try
            {
                var healthCheck = new MovementHealthCheck
                {
                    IsHealthy = true,
                    Timestamp = DateTime.UtcNow,
                    Checks = new List<HealthCheckItem>()
                };

                // Check database tables exist
                var hasUnitDeployments = await _context.UnitDeployments.AnyAsync();
                healthCheck.Checks.Add(new HealthCheckItem
                {
                    Name = "UnitDeployments Table",
                    Status = hasUnitDeployments ? "OK" : "WARNING",
                    Message = hasUnitDeployments ? "Table exists with data" : "Table empty"
                });

                var hasTerrainTypes = await _context.TerrainTypes.AnyAsync();
                healthCheck.Checks.Add(new HealthCheckItem
                {
                    Name = "TerrainTypes Table",
                    Status = hasTerrainTypes ? "OK" : "WARNING",
                    Message = hasTerrainTypes ? "Terrain types available" : "No terrain types configured"
                });

                var hasRoutesDrafts = await _context.RoutesDrafts.AnyAsync();
                healthCheck.Checks.Add(new HealthCheckItem
                {
                    Name = "RoutesDrafts Table",
                    Status = "OK",
                    Message = hasRoutesDrafts ? "Route drafts available" : "No route drafts yet"
                });

                return Ok(healthCheck);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new MovementHealthCheck
                {
                    IsHealthy = false,
                    Timestamp = DateTime.UtcNow,
                    Error = ex.Message
                });
            }
        }

        #region Helper Methods

        private double CalculateDistance(double lat1, double lng1, double lat2, double lng2)
        {
            // Simplified distance calculation - in real implementation, use proper geodetic calculation
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLng = (lng2 - lng1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var earthRadius = 6371; // Earth's radius in kilometers
            return earthRadius * c;
        }

        private string GetSupplyImpact(double movementCost)
        {
            if (movementCost <= 20) return "None";
            if (movementCost <= 40) return "Light";
            if (movementCost <= 60) return "Moderate";
            return "Heavy";
        }

        #endregion
    }

    #region DTOs

    public class UnitMovementData
    {
        public Guid UnitId { get; set; }
        public string UnitName { get; set; }
        public string UnitType { get; set; }
        public int MovementPointsPerTurn { get; set; }
        public int RemainingMovementPoints { get; set; }
        public string CurrentTerrain { get; set; }
        public string SupplyState { get; set; }
        public double CombatPowerIndex { get; set; }
        public double EffectiveCombatPower { get; set; }
        public Position Position { get; set; }
    }


    public class MovementCostRequest
    {
        public List<Position> Waypoints { get; set; }
        public string TerrainType { get; set; }
        public string MovementType { get; set; }
    }

    public class MovementCostResult
    {
        public double TotalDistanceKm { get; set; }
        public double TotalMovementCost { get; set; }
        public double EstimatedTimeHours { get; set; }
        public string SupplyImpact { get; set; }
        public bool IsValid { get; set; }
    }

    public class MovementValidationRequest
    {
        public Guid UnitId { get; set; }
        public double RequiredMovementCost { get; set; }
    }

    public class MovementValidationResult
    {
        public Guid UnitId { get; set; }
        public double AvailableMovement { get; set; }
        public double RequiredMovement { get; set; }
        public bool CanMove { get; set; }
        public double RemainingAfterMove { get; set; }
        public List<string> Warnings { get; set; }
    }

    public class RouteDraftRequest
    {
        public Guid UnitId { get; set; }
        public string RouteName { get; set; }
        public List<Position> Waypoints { get; set; }
        public double TotalDistanceKm { get; set; }
        public double EstimatedTimeHours { get; set; }
        public double? SupplyImpact { get; set; }
    }

    public class RouteDraftResult
    {
        public Guid RouteId { get; set; }
        public string RouteName { get; set; }
        public double TotalDistanceKm { get; set; }
        public DateTime? ETA { get; set; }
        public bool IsSaved { get; set; }
    }

    public class MovementCommitRequest
    {
        public Guid UnitId { get; set; }
        public string ActionType { get; set; }
        public List<Position> Waypoints { get; set; }
        public double MovementCost { get; set; }
        public string TerrainType { get; set; }
    }

    public class MovementCommitResult
    {
        public Guid UnitId { get; set; }
        public string ActionType { get; set; }
        public double MovementCost { get; set; }
        public int RemainingMovement { get; set; }
        public bool IsCommitted { get; set; }
        public Position NewPosition { get; set; }
        public double EffectiveCombatPower { get; set; }
    }

    public class MovementHealthCheck
    {
        public bool IsHealthy { get; set; }
        public DateTime Timestamp { get; set; }
        public List<HealthCheckItem> Checks { get; set; }
        public string Error { get; set; }
    }

    public class HealthCheckItem
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }

    #endregion
}
