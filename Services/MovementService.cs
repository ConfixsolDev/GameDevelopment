using TechWebSol.Models;

namespace TechWebSol.Services
{
    public class MovementService
    {
        public double CalculateMovementCost(string terrainType, double baseMovement)
        {
            return terrainType switch
            {
                "Road" => baseMovement * 1.0,
                "CrossCountry" => baseMovement * 0.7,
                "Forest" => baseMovement * 0.5,
                "Mountain" => baseMovement * 0.3,
                "River" => baseMovement * 0.4,
                "Urban" => baseMovement * 0.6,
                "Desert" => baseMovement * 0.8,
                "Swamp" => baseMovement * 0.3,
                _ => baseMovement * 0.8
            };
        }

        public double CalculateMovementCost(string terrainType, double baseMovement, int supplyState)
        {
            var terrainCost = CalculateMovementCost(terrainType, baseMovement);
            var supplyModifier = GetSupplyModifier(supplyState);
            return terrainCost * supplyModifier;
        }

        public bool CanMove(MilitaryUnit unit, double distance, string terrainType)
        {
            var requiredMovement = CalculateMovementCost(terrainType, distance, unit.SupplyState);
            return unit.RemainingMovement >= requiredMovement;
        }

        public bool CanMove(UnitDeployment deployment, double distance, string terrainType)
        {
            var requiredMovement = CalculateMovementCost(terrainType, distance, deployment.SupplyState);
            return deployment.RemainingMovement >= requiredMovement;
        }

        public double GetEffectiveMovement(MilitaryUnit unit, string terrainType)
        {
            var baseMovement = unit.MovementPoints;
            return CalculateMovementCost(terrainType, baseMovement, unit.SupplyState);
        }

        public double GetEffectiveMovement(UnitDeployment deployment, string terrainType)
        {
            var baseMovement = deployment.MovementPoints;
            return CalculateMovementCost(terrainType, baseMovement, deployment.SupplyState);
        }

        public void ConsumeMovement(MilitaryUnit unit, double distance, string terrainType)
        {
            var movementCost = CalculateMovementCost(terrainType, distance, unit.SupplyState);
            unit.RemainingMovement = Math.Max(0, unit.RemainingMovement - movementCost);
            unit.CurrentTerrain = terrainType;
        }

        public void ConsumeMovement(UnitDeployment deployment, double distance, string terrainType)
        {
            var movementCost = CalculateMovementCost(terrainType, distance, deployment.SupplyState);
            deployment.RemainingMovement = Math.Max(0, deployment.RemainingMovement - movementCost);
            deployment.CurrentTerrain = terrainType;
        }

        public void ResetMovementPoints(MilitaryUnit unit)
        {
            unit.RemainingMovement = unit.MovementPoints;
        }

        public void ResetMovementPoints(UnitDeployment deployment)
        {
            deployment.RemainingMovement = deployment.MovementPoints;
        }

        private double GetSupplyModifier(int supplyState)
        {
            return supplyState switch
            {
                100 => 1.0,    // Green
                75 => 0.75,    // Amber
                50 => 0.5,     // Red
                _ => 1.0
            };
        }

        public string GetTerrainAtPosition(string position)
        {
            // Simplified terrain detection - in real implementation would query map data
            // For now, return default terrain
            return "Road";
        }
    }
}
