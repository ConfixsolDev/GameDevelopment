using TechWebSol.Models;

namespace TechWebSol.Services
{
    public class CombatService
    {
        private readonly MovementService _movementService;

        public CombatService(MovementService movementService)
        {
            _movementService = movementService;
        }

        public CombatResult ResolveCombat(MilitaryUnit attacker, MilitaryUnit defender, string terrainType)
        {
            var attackerEffectiveness = CalculateEffectiveness(attacker, terrainType);
            var defenderEffectiveness = CalculateEffectiveness(defender, terrainType);

            var attackerCasualties = CalculateCasualties(defenderEffectiveness, attackerEffectiveness);
            var defenderCasualties = CalculateCasualties(attackerEffectiveness, defenderEffectiveness);

            // Apply casualties to strength
            attacker.StrengthPercentage = Math.Max(0, attacker.StrengthPercentage - attackerCasualties);
            defender.StrengthPercentage = Math.Max(0, defender.StrengthPercentage - defenderCasualties);

            // Update effective combat power
            attacker.EffectiveCombatPower = attacker.GetEffectiveCombatPower();
            defender.EffectiveCombatPower = defender.GetEffectiveCombatPower();

            return new CombatResult
            {
                AttackerCasualties = attackerCasualties,
                DefenderCasualties = defenderCasualties,
                AttackerEffectiveness = attackerEffectiveness,
                DefenderEffectiveness = defenderEffectiveness,
                TerrainType = terrainType
            };
        }

        private double CalculateEffectiveness(MilitaryUnit unit, string terrainType)
        {
            var terrainModifier = GetTerrainModifier(terrainType, unit.UnitType);
            var supplyModifier = GetSupplyModifier(unit.SupplyState);
            return unit.CombatPower * unit.TerrainModifier * supplyModifier * terrainModifier;
        }

        private double CalculateCasualties(double enemyEffectiveness, double ownEffectiveness)
        {
            if (ownEffectiveness <= 0) return 50; // Maximum casualties if no effectiveness

            var casualtyRatio = enemyEffectiveness / ownEffectiveness;
            var baseCasualties = Math.Min(50, casualtyRatio * 20); // Cap at 50%
            
            // Add some randomness for realism
            var randomFactor = new Random().NextDouble() * 0.2 + 0.9; // 0.9 to 1.1
            return Math.Max(1, baseCasualties * randomFactor);
        }

        private double GetTerrainModifier(string terrainType, string unitType)
        {
            return terrainType?.ToLower() switch
            {
                "plain" => 1.0,
                "forest" => unitType?.ToLower() switch
                {
                    "infantry" => 1.2, // Infantry advantage in forest
                    "armoured" => 0.7, // Armour disadvantage
                    _ => 0.8
                },
                "mountain" => unitType?.ToLower() switch
                {
                    "infantry" => 1.1,
                    "armoured" => 0.5,
                    "artillery" => 0.8,
                    _ => 0.7
                },
                "urban" => unitType?.ToLower() switch
                {
                    "infantry" => 1.3, // Infantry advantage in urban
                    "armoured" => 0.6,
                    _ => 0.8
                },
                "desert" => unitType?.ToLower() switch
                {
                    "armoured" => 1.1, // Armour advantage in desert
                    "infantry" => 0.8,
                    _ => 0.9
                },
                _ => 1.0
            };
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
    }

    public class CombatResult
    {
        public double AttackerCasualties { get; set; }
        public double DefenderCasualties { get; set; }
        public double AttackerEffectiveness { get; set; }
        public double DefenderEffectiveness { get; set; }
        public string TerrainType { get; set; } = string.Empty;
        public DateTime CombatTime { get; set; } = DateTime.UtcNow;
    }
}
