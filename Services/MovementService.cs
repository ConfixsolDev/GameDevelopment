using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Models;

namespace TechWebSol.Services
{
    public interface IMovementService
    {
        Task<MovementBudgetResult> CalculateMovementBudgetAsync(Guid unitId);
        Task<RouteValidationResult> ValidateRouteAsync(Guid unitId, List<Waypoint> waypoints);
        Task<decimal> CalculateRouteCostAsync(List<Waypoint> waypoints, string terrainType);
        Task<bool> CommitRouteAsync(Guid routeId);
        Task<List<TerrainType>> GetDefaultTerrainTypesAsync();
    }

    public class MovementService : IMovementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MovementService> _logger;

        public MovementService(ApplicationDbContext context, ILogger<MovementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MovementBudgetResult> CalculateMovementBudgetAsync(Guid unitId)
        {
            // UnitDeployment removed - movement system needs refactoring for Brigade system
            await Task.CompletedTask; // Suppress async warning
            _logger.LogWarning("Movement system temporarily unavailable - UnitDeployment replaced with Brigade system");
            return new MovementBudgetResult
            {
                Success = false,
                ErrorMessage = "Movement system temporarily unavailable - UnitDeployment system replaced with Brigade system. Please use token movement instead."
            };
        }

        public async Task<RouteValidationResult> ValidateRouteAsync(Guid unitId, List<Waypoint> waypoints)
        {
            // Implementation will be added in next part
            throw new NotImplementedException();
        }

        public async Task<decimal> CalculateRouteCostAsync(List<Waypoint> waypoints, string terrainType)
        {
            // Implementation will be added in next part
            throw new NotImplementedException();
        }

        public async Task<bool> CommitRouteAsync(Guid routeId)
        {
            // Implementation will be added in next part
            throw new NotImplementedException();
        }

        public async Task<List<TerrainType>> GetDefaultTerrainTypesAsync()
        {
            var defaultTerrains = new List<TerrainType>
            {
                new TerrainType { TerrainCode = "OPEN", Name = "Open Terrain", MovementCostRoad = 1.0m, MovementCostCrossCountry = 1.2m, CombatModifier = 1.0m },
                new TerrainType { TerrainCode = "FOREST", Name = "Forest", MovementCostRoad = 1.2m, MovementCostCrossCountry = 1.6m, CombatModifier = 0.9m },
                new TerrainType { TerrainCode = "MOUNTAIN", Name = "Mountain", MovementCostRoad = 1.6m, MovementCostCrossCountry = 2.2m, CombatModifier = 0.85m },
                new TerrainType { TerrainCode = "RIVER", Name = "River", MovementCostRoad = 2.0m, MovementCostCrossCountry = 3.0m, CombatModifier = 0.8m }
            };

            return defaultTerrains;
        }

        // Additional methods for SimulationController compatibility
        public async Task<string> GetTerrainAtPosition(double lat, double lng)
        {
            // Simplified implementation - in real scenario, this would query terrain data
            return "OPEN";
        }

        public async Task<double> CalculateMovementCost(string terrainType, string movementType)
        {
            var terrain = await _context.TerrainTypes
                .FirstOrDefaultAsync(t => t.TerrainCode == terrainType);

            if (terrain == null)
            {
                return terrainType.ToUpper() switch
                {
                    "OPEN" => 1.0,
                    "FOREST" => movementType == "road" ? 1.2 : 1.6,
                    "MOUNTAIN" => movementType == "road" ? 1.6 : 2.2,
                    "RIVER" => movementType == "road" ? 2.0 : 3.0,
                    _ => 1.0
                };
            }

            return movementType == "road" ? (double)terrain.MovementCostRoad : (double)terrain.MovementCostCrossCountry;
        }

        [Obsolete("UnitDeployment removed - use Brigade system")]
        public async Task<double> GetEffectiveMovement(UnitDeployment unit)
        {
            await Task.CompletedTask;
            return 0; // Stub - UnitDeployment removed
        }

        [Obsolete("UnitDeployment removed - use Brigade system")]
        public async Task<bool> CanMove(UnitDeployment unit, double distance)
        {
            await Task.CompletedTask;
            return false; // Stub - UnitDeployment removed
        }

        private async Task<decimal> GetTerrainMovementCostAsync(string terrainCode, string movementType)
        {
            var terrain = await _context.TerrainTypes
                .FirstOrDefaultAsync(t => t.TerrainCode == terrainCode);

            if (terrain == null)
            {
                // Return default cost if terrain not found
                return terrainCode.ToUpper() switch
                {
                    "OPEN" => 1.0m,
                    "FOREST" => movementType == "road" ? 1.2m : 1.6m,
                    "MOUNTAIN" => movementType == "road" ? 1.6m : 2.2m,
                    "RIVER" => movementType == "road" ? 2.0m : 3.0m,
                    _ => 1.0m
                };
            }

            return movementType == "road" ? terrain.MovementCostRoad : terrain.MovementCostCrossCountry;
        }

        private double GetSupplyModifier(string supplyState)
        {
            return supplyState?.ToUpper() switch
            {
                "GREEN" => 1.0,
                "AMBER" => 0.85,
                "RED" => 0.6,
                _ => 1.0
            };
        }
    }

    // DTOs for movement calculations
    public class MovementBudgetResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public Guid UnitId { get; set; }
        public int BaseMovementBudget { get; set; }
        public double SupplyModifier { get; set; }
        public double SupplyAdjustedBudget { get; set; }
        public string CurrentTerrain { get; set; }
        public decimal TerrainCost { get; set; }
        public string SupplyState { get; set; }
        public double EffectiveCombatPower { get; set; }
    }

    public class RouteValidationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public Guid UnitId { get; set; }
        public decimal TotalRouteCost { get; set; }
        public double AvailableBudget { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public int EstimatedTime { get; set; }
    }

    public class Waypoint
    {
        public string Grid { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public string Terrain { get; set; }
        public string Via { get; set; } = "road"; // road, cross_country
    }
}