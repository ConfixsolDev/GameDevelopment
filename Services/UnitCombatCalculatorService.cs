using TechWebSol.Models;
using TechWebSol.Data;
using Microsoft.EntityFrameworkCore;

namespace TechWebSol.Services
{
    /// <summary>
    /// Unit-level combat calculator that aggregates weapon effects
    /// Based on FM 3-0 Operations, ATP 3-90.1, FM 6-0 Commander and Staff Organization and Operations
    /// </summary>
    public interface IUnitCombatCalculatorService
    {
        Task<UnitCombatResult> CalculateUnitVsUnitCombatAsync(Guid attackerTokenId, Guid defenderTokenId, CombatContext context);
    }

    public class UnitCombatCalculatorService : IUnitCombatCalculatorService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWeaponEffectivenessService _weaponEffectiveness;
        private readonly ILogger<UnitCombatCalculatorService> _logger;

        public UnitCombatCalculatorService(
            ApplicationDbContext context,
            IWeaponEffectivenessService weaponEffectiveness,
            ILogger<UnitCombatCalculatorService> logger)
        {
            _context = context;
            _weaponEffectiveness = weaponEffectiveness;
            _logger = logger;
        }

        /// <summary>
        /// Calculate comprehensive unit vs unit combat
        /// Aggregates all weapons in both units and calculates expected outcomes
        /// </summary>
        public async Task<UnitCombatResult> CalculateUnitVsUnitCombatAsync(Guid attackerTokenId, Guid defenderTokenId, CombatContext context)
        {
            var result = new UnitCombatResult
            {
                AttackerTokenId = attackerTokenId,
                DefenderTokenId = defenderTokenId,
                Context = context
            };

            try
            {
                // Get attacker and defender data
                var attackerToken = await _context.Tokens.FindAsync(attackerTokenId);
                var defenderToken = await _context.Tokens.FindAsync(defenderTokenId);

                if (attackerToken == null || defenderToken == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "Token not found";
                    return result;
                }

                // Get brigades directly using TokenId
                var attackerBrigade = await GetBrigadeDataByToken(attackerTokenId);
                var defenderBrigade = await GetBrigadeDataByToken(defenderTokenId);

                if (attackerBrigade == null)
                {
                    _logger.LogError($"❌ CRITICAL: NO MILITARY UNITS FOUND for attacker token '{attackerToken.Name}' ({attackerTokenId})");
                    throw new InvalidOperationException($"Attacker token '{attackerToken.Name}' has no military units (brigade or direct units). Cannot calculate weapon-level combat. Please add Infantry, Armoured, or Artillery units to this token in Data Management.");
                }
                
                _logger.LogInformation($"✅ Found attacker '{attackerToken.Name}' units: {attackerBrigade.InfantryBattalions.Count} infantry, {attackerBrigade.ArmouredRegiments.Count} armour, {attackerBrigade.ArtilleryRegiments.Count} artillery, {attackerBrigade.Engineers.Count} engineers");
                
                if (defenderBrigade == null)
                {
                    _logger.LogError($"❌ CRITICAL: NO MILITARY UNITS FOUND for defender token '{defenderToken.Name}' ({defenderTokenId})");
                    throw new InvalidOperationException($"Defender token '{defenderToken.Name}' has no military units (brigade or direct units). Cannot calculate weapon-level combat. Please add Infantry, Armoured, or Artillery units to this token in Data Management.");
                }
                
                _logger.LogInformation($"✅ Found defender '{defenderToken.Name}' units: {defenderBrigade.InfantryBattalions.Count} infantry, {defenderBrigade.ArmouredRegiments.Count} armour, {defenderBrigade.ArtilleryRegiments.Count} artillery, {defenderBrigade.Engineers.Count} engineers");

                // Calculate range between units
                var attackerMarker = attackerToken.MapMarkers?.FirstOrDefault(m => m.IsActive);
                var defenderMarker = defenderToken.MapMarkers?.FirstOrDefault(m => m.IsActive);

                double range = 1000; // Default 1km
                if (attackerMarker != null && defenderMarker != null)
                {
                    range = CalculateDistance(
                        double.Parse(attackerMarker.latitude),
                        double.Parse(attackerMarker.longitude),
                        double.Parse(defenderMarker.latitude),
                        double.Parse(defenderMarker.longitude)
                    ) * 1000; // Convert km to meters
                }

                // Determine target type for defender
                var defenderType = DetermineUnitType(defenderToken, defenderBrigade);

                // Calculate attacker fires (all weapons)
                result.AttackerWeaponResults = await CalculateAttackerFiresAsync(
                    attackerBrigade, 
                    defenderType, 
                    range, 
                    context);

                // Calculate defender return fires
                var attackerType = DetermineUnitType(attackerToken, attackerBrigade);
                result.DefenderWeaponResults = await CalculateDefenderFiresAsync(
                    defenderBrigade, 
                    attackerType, 
                    range, 
                    context);

                // Aggregate results
                result.TotalAttackerCasualtiesInflicted = result.AttackerWeaponResults.Sum(w => w.TotalCasualties);
                result.TotalDefenderCasualtiesInflicted = result.DefenderWeaponResults.Sum(w => w.TotalCasualties);
                result.TotalEngagementTimeSeconds = Math.Max(
                    result.AttackerWeaponResults.Any() ? result.AttackerWeaponResults.Max(w => w.TimeToEngageSeconds) : 0,
                    result.DefenderWeaponResults.Any() ? result.DefenderWeaponResults.Max(w => w.TimeToEngageSeconds) : 0
                );

                result.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating unit vs unit combat");
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Calculate all attacker weapon fires
        /// </summary>
        private async Task<List<WeaponSystemResult>> CalculateAttackerFiresAsync(
            BrigadeWeaponData? brigade,
            string targetType,
            double range,
            CombatContext context)
        {
            var results = new List<WeaponSystemResult>();

            if (brigade == null)
                throw new InvalidOperationException("Brigade data is null - cannot calculate weapon fires");

            // Infantry battalion weapons
            foreach (var infantry in brigade.InfantryBattalions)
            {
                results.AddRange(CalculateInfantryWeaponFires(infantry, targetType, range, context));
            }

            // Armoured regiment weapons
            foreach (var armour in brigade.ArmouredRegiments)
            {
                results.AddRange(CalculateArmourWeaponFires(armour, targetType, range, context));
            }

            // Artillery regiment weapons
            foreach (var artillery in brigade.ArtilleryRegiments)
            {
                results.AddRange(CalculateArtilleryWeaponFires(artillery, targetType, range, context));
            }

            // Engineer weapons
            foreach (var engineer in brigade.Engineers)
            {
                results.AddRange(CalculateEngineerWeaponFires(engineer, targetType, range, context));
            }

            return results;
        }

        /// <summary>
        /// Calculate defender return fires (same logic, just from defender's perspective)
        /// </summary>
        private async Task<List<WeaponSystemResult>> CalculateDefenderFiresAsync(
            BrigadeWeaponData? brigade,
            string targetType,
            double range,
            CombatContext context)
        {
            // Apply defender advantage modifier (in prepared positions)
            var modifiedContext = new CombatContext
            {
                Terrain = context.Terrain,
                Weather = context.Weather,
                Visibility = context.Visibility,
                DefenderProtection = "improved", // Defenders typically have better protection
                AttackerPosture = "moving slow", // Attackers are moving
                DefenderPosture = "static"       // Defenders are static
            };

            return await CalculateAttackerFiresAsync(brigade, targetType, range, modifiedContext);
        }

        #region Infantry Weapons

        private List<WeaponSystemResult> CalculateInfantryWeaponFires(
            InfantryBattalion infantry,
            string targetType,
            double range,
            CombatContext context)
        {
            var results = new List<WeaponSystemResult>();

            // ATGMs
            if (infantry.ATGMS > 0)
            {
                var engagement = CreateEngagement("atgm", targetType, range, context);
                var effectiveness = _weaponEffectiveness.CalculateWeaponEffectiveness(engagement);
                results.Add(new WeaponSystemResult
                {
                    WeaponType = "ATGM",
                    Quantity = infantry.ATGMS,
                    CasualtiesPerWeapon = effectiveness.ExpectedCasualtiesPerWeapon,
                    TotalCasualties = effectiveness.ExpectedCasualtiesPerWeapon * infantry.ATGMS,
                    TimeToEngageSeconds = effectiveness.TimeToEngageSeconds,
                    FinalEffectiveness = effectiveness.FinalEffectiveness
                });
            }

            // Rocket Launchers
            if (infantry.RocketLauncher > 0)
            {
                var engagement = CreateEngagement("rocket", targetType, range, context);
                var effectiveness = _weaponEffectiveness.CalculateWeaponEffectiveness(engagement);
                results.Add(new WeaponSystemResult
                {
                    WeaponType = "Rocket Launcher",
                    Quantity = infantry.RocketLauncher,
                    CasualtiesPerWeapon = effectiveness.ExpectedCasualtiesPerWeapon,
                    TotalCasualties = effectiveness.ExpectedCasualtiesPerWeapon * infantry.RocketLauncher,
                    TimeToEngageSeconds = effectiveness.TimeToEngageSeconds,
                    FinalEffectiveness = effectiveness.FinalEffectiveness
                });
            }

            // 120mm Mortars
            if (infantry.Mortars120mm > 0)
            {
                var engagement = CreateEngagement("mortar", targetType, range, context);
                var effectiveness = _weaponEffectiveness.CalculateWeaponEffectiveness(engagement);
                results.Add(new WeaponSystemResult
                {
                    WeaponType = "120mm Mortar",
                    Quantity = infantry.Mortars120mm,
                    CasualtiesPerWeapon = effectiveness.ExpectedCasualtiesPerWeapon,
                    TotalCasualties = effectiveness.ExpectedCasualtiesPerWeapon * infantry.Mortars120mm,
                    TimeToEngageSeconds = effectiveness.TimeToEngageSeconds,
                    FinalEffectiveness = effectiveness.FinalEffectiveness
                });
            }

            // 81mm Mortars
            if (infantry.Mortars81mm > 0)
            {
                var engagement = CreateEngagement("mortar", targetType, range, context);
                var effectiveness = _weaponEffectiveness.CalculateWeaponEffectiveness(engagement);
                results.Add(new WeaponSystemResult
                {
                    WeaponType = "81mm Mortar",
                    Quantity = infantry.Mortars81mm,
                    CasualtiesPerWeapon = effectiveness.ExpectedCasualtiesPerWeapon * 0.7, // Less effective than 120mm
                    TotalCasualties = effectiveness.ExpectedCasualtiesPerWeapon * 0.7 * infantry.Mortars81mm,
                    TimeToEngageSeconds = effectiveness.TimeToEngageSeconds,
                    FinalEffectiveness = effectiveness.FinalEffectiveness * 0.7
                });
            }

            // Heavy MGs
            if (infantry.HMG_AGL > 0)
            {
                var engagement = CreateEngagement("hmg", targetType, range, context);
                var effectiveness = _weaponEffectiveness.CalculateWeaponEffectiveness(engagement);
                results.Add(new WeaponSystemResult
                {
                    WeaponType = "HMG/AGL",
                    Quantity = infantry.HMG_AGL,
                    CasualtiesPerWeapon = effectiveness.ExpectedCasualtiesPerWeapon,
                    TotalCasualties = effectiveness.ExpectedCasualtiesPerWeapon * infantry.HMG_AGL,
                    TimeToEngageSeconds = effectiveness.TimeToEngageSeconds,
                    FinalEffectiveness = effectiveness.FinalEffectiveness
                });
            }

            // MGs/LMGs
            if (infantry.MG_LMG > 0)
            {
                var engagement = CreateEngagement("hmg", targetType, range, context);
                var effectiveness = _weaponEffectiveness.CalculateWeaponEffectiveness(engagement);
                results.Add(new WeaponSystemResult
                {
                    WeaponType = "MG/LMG",
                    Quantity = infantry.MG_LMG,
                    CasualtiesPerWeapon = effectiveness.ExpectedCasualtiesPerWeapon * 0.8, // Slightly less than HMG
                    TotalCasualties = effectiveness.ExpectedCasualtiesPerWeapon * 0.8 * infantry.MG_LMG,
                    TimeToEngageSeconds = effectiveness.TimeToEngageSeconds,
                    FinalEffectiveness = effectiveness.FinalEffectiveness * 0.8
                });
            }

            // Grenade Launchers
            if (infantry.GrenadeLaunchers > 0)
            {
                var engagement = CreateEngagement("grenade", targetType, range, context);
                var effectiveness = _weaponEffectiveness.CalculateWeaponEffectiveness(engagement);
                results.Add(new WeaponSystemResult
                {
                    WeaponType = "Grenade Launcher",
                    Quantity = infantry.GrenadeLaunchers,
                    CasualtiesPerWeapon = effectiveness.ExpectedCasualtiesPerWeapon,
                    TotalCasualties = effectiveness.ExpectedCasualtiesPerWeapon * infantry.GrenadeLaunchers,
                    TimeToEngageSeconds = effectiveness.TimeToEngageSeconds,
                    FinalEffectiveness = effectiveness.FinalEffectiveness
                });
            }

            // Infantry rifles (estimated from strength)
            var rifleCount = infantry.Strength * 0.7; // 70% are riflemen
            if (rifleCount > 0)
            {
                var engagement = CreateEngagement("infantry", targetType, range, context);
                var effectiveness = _weaponEffectiveness.CalculateWeaponEffectiveness(engagement);
                results.Add(new WeaponSystemResult
                {
                    WeaponType = "Infantry Rifles",
                    Quantity = (int)rifleCount,
                    CasualtiesPerWeapon = effectiveness.ExpectedCasualtiesPerWeapon,
                    TotalCasualties = effectiveness.ExpectedCasualtiesPerWeapon * rifleCount,
                    TimeToEngageSeconds = effectiveness.TimeToEngageSeconds,
                    FinalEffectiveness = effectiveness.FinalEffectiveness
                });
            }

            return results;
        }

        #endregion

        #region Armour Weapons

        private List<WeaponSystemResult> CalculateArmourWeaponFires(
            ArmouredRegiment armour,
            string targetType,
            double range,
            CombatContext context)
        {
            var results = new List<WeaponSystemResult>();

            // Tank main guns
            if (armour.Tanks > 0)
            {
                var engagement = CreateEngagement("tank", targetType, range, context);
                var effectiveness = _weaponEffectiveness.CalculateWeaponEffectiveness(engagement);
                results.Add(new WeaponSystemResult
                {
                    WeaponType = "Tank Main Gun",
                    Quantity = armour.Tanks,
                    CasualtiesPerWeapon = effectiveness.ExpectedCasualtiesPerWeapon,
                    TotalCasualties = effectiveness.ExpectedCasualtiesPerWeapon * armour.Tanks,
                    TimeToEngageSeconds = effectiveness.TimeToEngageSeconds,
                    FinalEffectiveness = effectiveness.FinalEffectiveness
                });
            }

            // Armour ATGMs
            if (armour.ATGMS > 0)
            {
                var engagement = CreateEngagement("atgm", targetType, range, context);
                var effectiveness = _weaponEffectiveness.CalculateWeaponEffectiveness(engagement);
                results.Add(new WeaponSystemResult
                {
                    WeaponType = "ATGM (Armour)",
                    Quantity = armour.ATGMS,
                    CasualtiesPerWeapon = effectiveness.ExpectedCasualtiesPerWeapon,
                    TotalCasualties = effectiveness.ExpectedCasualtiesPerWeapon * armour.ATGMS,
                    TimeToEngageSeconds = effectiveness.TimeToEngageSeconds,
                    FinalEffectiveness = effectiveness.FinalEffectiveness
                });
            }

            // 120mm Mortars
            if (armour.Mortars120mm > 0)
            {
                var engagement = CreateEngagement("mortar", targetType, range, context);
                var effectiveness = _weaponEffectiveness.CalculateWeaponEffectiveness(engagement);
                results.Add(new WeaponSystemResult
                {
                    WeaponType = "120mm Mortar (Armour)",
                    Quantity = armour.Mortars120mm,
                    CasualtiesPerWeapon = effectiveness.ExpectedCasualtiesPerWeapon,
                    TotalCasualties = effectiveness.ExpectedCasualtiesPerWeapon * armour.Mortars120mm,
                    TimeToEngageSeconds = effectiveness.TimeToEngageSeconds,
                    FinalEffectiveness = effectiveness.FinalEffectiveness
                });
            }

            // Tank HMGs
            if (armour.HMG > 0)
            {
                var engagement = CreateEngagement("hmg", targetType, range, context);
                var effectiveness = _weaponEffectiveness.CalculateWeaponEffectiveness(engagement);
                results.Add(new WeaponSystemResult
                {
                    WeaponType = "HMG (Tank)",
                    Quantity = armour.HMG,
                    CasualtiesPerWeapon = effectiveness.ExpectedCasualtiesPerWeapon,
                    TotalCasualties = effectiveness.ExpectedCasualtiesPerWeapon * armour.HMG,
                    TimeToEngageSeconds = effectiveness.TimeToEngageSeconds,
                    FinalEffectiveness = effectiveness.FinalEffectiveness
                });
            }

            return results;
        }

        #endregion

        #region Artillery Weapons

        private List<WeaponSystemResult> CalculateArtilleryWeaponFires(
            ArtilleryRegiment artillery,
            string targetType,
            double range,
            CombatContext context)
        {
            var results = new List<WeaponSystemResult>();

            // Artillery guns
            if (artillery.Guns > 0)
            {
                var engagement = CreateEngagement("artillery", targetType, range, context);
                var effectiveness = _weaponEffectiveness.CalculateWeaponEffectiveness(engagement);
                
                // Artillery fires multiple rounds per gun
                var roundsPerGun = 6; // Average fire for effect mission
                
                results.Add(new WeaponSystemResult
                {
                    WeaponType = $"Artillery {artillery.GunCaliber}",
                    Quantity = artillery.Guns,
                    CasualtiesPerWeapon = effectiveness.ExpectedCasualtiesPerWeapon * roundsPerGun,
                    TotalCasualties = effectiveness.ExpectedCasualtiesPerWeapon * roundsPerGun * artillery.Guns,
                    TimeToEngageSeconds = effectiveness.TimeToEngageSeconds,
                    FinalEffectiveness = effectiveness.FinalEffectiveness
                });
            }

            return results;
        }

        #endregion

        #region Engineer Weapons

        private List<WeaponSystemResult> CalculateEngineerWeaponFires(
            CombatEngineeringCompany engineer,
            string targetType,
            double range,
            CombatContext context)
        {
            var results = new List<WeaponSystemResult>();

            // Engineer ATGMs
            if (engineer.ATGMS > 0)
            {
                var engagement = CreateEngagement("atgm", targetType, range, context);
                var effectiveness = _weaponEffectiveness.CalculateWeaponEffectiveness(engagement);
                results.Add(new WeaponSystemResult
                {
                    WeaponType = "ATGM (Engineer)",
                    Quantity = engineer.ATGMS,
                    CasualtiesPerWeapon = effectiveness.ExpectedCasualtiesPerWeapon,
                    TotalCasualties = effectiveness.ExpectedCasualtiesPerWeapon * engineer.ATGMS,
                    TimeToEngageSeconds = effectiveness.TimeToEngageSeconds,
                    FinalEffectiveness = effectiveness.FinalEffectiveness
                });
            }

            // Engineer HMGs
            if (engineer.HMG > 0)
            {
                var engagement = CreateEngagement("hmg", targetType, range, context);
                var effectiveness = _weaponEffectiveness.CalculateWeaponEffectiveness(engagement);
                results.Add(new WeaponSystemResult
                {
                    WeaponType = "HMG (Engineer)",
                    Quantity = engineer.HMG,
                    CasualtiesPerWeapon = effectiveness.ExpectedCasualtiesPerWeapon,
                    TotalCasualties = effectiveness.ExpectedCasualtiesPerWeapon * engineer.HMG,
                    TimeToEngageSeconds = effectiveness.TimeToEngageSeconds,
                    FinalEffectiveness = effectiveness.FinalEffectiveness
                });
            }

            // Engineer LMGs
            if (engineer.LMG > 0)
            {
                var engagement = CreateEngagement("hmg", targetType, range, context);
                var effectiveness = _weaponEffectiveness.CalculateWeaponEffectiveness(engagement);
                results.Add(new WeaponSystemResult
                {
                    WeaponType = "LMG (Engineer)",
                    Quantity = engineer.LMG,
                    CasualtiesPerWeapon = effectiveness.ExpectedCasualtiesPerWeapon * 0.7,
                    TotalCasualties = effectiveness.ExpectedCasualtiesPerWeapon * 0.7 * engineer.LMG,
                    TimeToEngageSeconds = effectiveness.TimeToEngageSeconds,
                    FinalEffectiveness = effectiveness.FinalEffectiveness * 0.7
                });
            }

            return results;
        }

        #endregion

        #region Helper Methods

        private WeaponEngagement CreateEngagement(string weaponType, string targetType, double range, CombatContext context)
        {
            return new WeaponEngagement
            {
                WeaponType = weaponType,
                TargetType = targetType,
                Range = range,
                Terrain = context.Terrain,
                Weather = context.Weather,
                Visibility = context.Visibility,
                TargetProtection = context.DefenderProtection,
                TargetPosture = context.DefenderPosture
            };
        }

        private async Task<BrigadeWeaponData?> GetBrigadeDataByToken(Guid tokenId)
        {
            // Get token name for better logging
            var token = await _context.Tokens.FindAsync(tokenId);
            var tokenName = token?.Name ?? tokenId.ToString();
            
            // Try to find Brigade with TokenId
            var brigade = await _context.Brigades
                .Include(b => b.InfantryBattalions)
                .Include(b => b.ArmouredRegiments)
                .Include(b => b.ArtilleryRegiments)
                .Include(b => b.CombatEngineeringCompanies)
                .FirstOrDefaultAsync(b => b.TokenId == tokenId);

            // If no brigade found, check for DIRECT UNITS (units without brigade)
            if (brigade == null)
            {
                _logger.LogInformation($"No brigade found for token '{tokenName}', checking for direct units...");
                
                // Load direct units (units with TokenId but no BrigadeId)
                var directInfantry = await _context.InfantryBattalions
                    .Where(i => i.TokenId == tokenId && i.IsActive)
                    .ToListAsync();
                    
                var directArmoured = await _context.ArmouredRegiments
                    .Where(a => a.TokenId == tokenId && a.IsActive && !a.IsDeleted)
                    .ToListAsync();
                    
                var directArtillery = await _context.ArtilleryRegiments
                    .Where(a => a.TokenId == tokenId && a.IsActive)
                    .ToListAsync();
                    
                var directEngineers = await _context.CombatEngineeringCompanies
                    .Where(e => e.TokenId == tokenId && e.IsActive)
                    .ToListAsync();
                
                // If no direct units either, return null
                if (!directInfantry.Any() && !directArmoured.Any() && !directArtillery.Any() && !directEngineers.Any())
                {
                    _logger.LogWarning($"⚠️ No brigade AND no direct units found for token '{tokenName}'");
                    return null;
                }
                
                _logger.LogInformation($"✅ Found direct units for token '{tokenName}': {directInfantry.Count} infantry, {directArmoured.Count} armour, {directArtillery.Count} artillery, {directEngineers.Count} engineers");
                
                // Return direct units as BrigadeWeaponData
                return new BrigadeWeaponData
                {
                    InfantryBattalions = directInfantry,
                    ArmouredRegiments = directArmoured,
                    ArtilleryRegiments = directArtillery,
                    Engineers = directEngineers
                };
            }

            // Brigade found - return its units
            _logger.LogInformation($"✅ Found brigade for token '{tokenName}': {brigade.InfantryBattalions.Count} infantry, {brigade.ArmouredRegiments.Count} armour, {brigade.ArtilleryRegiments.Count} artillery, {brigade.CombatEngineeringCompanies.Count} engineers");
            
            return new BrigadeWeaponData
            {
                InfantryBattalions = brigade.InfantryBattalions.ToList(),
                ArmouredRegiments = brigade.ArmouredRegiments.Where(a => !a.IsDeleted).ToList(),
                ArtilleryRegiments = brigade.ArtilleryRegiments.ToList(),
                Engineers = brigade.CombatEngineeringCompanies.ToList()
            };
        }

        private string DetermineUnitType(Token token, BrigadeWeaponData? brigade)
        {
            if (brigade == null)
                return "infantry";

            // Determine primary unit type based on composition
            if (brigade.ArmouredRegiments.Any() && brigade.ArmouredRegiments.Sum(a => a.Tanks) > 50)
                return "tank";
            else if (brigade.InfantryBattalions.Any())
                return "infantry";
            else if (brigade.ArtilleryRegiments.Any())
                return "artillery";
            else
                return "infantry";
        }

        private double CalculateDistance(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371; // Earth's radius in kilometers
            var dLat = ToRadians(lat2 - lat1);
            var dLng = ToRadians(lng2 - lng1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        #endregion
    }

    #region Data Models

    public class CombatContext
    {
        public string Terrain { get; set; } = "open";
        public string Weather { get; set; } = "clear";
        public string Visibility { get; set; } = "day clear";
        public string DefenderProtection { get; set; } = "none";
        public string AttackerPosture { get; set; } = "moving slow";
        public string DefenderPosture { get; set; } = "static";
    }

    public class BrigadeWeaponData
    {
        public List<InfantryBattalion> InfantryBattalions { get; set; } = new List<InfantryBattalion>();
        public List<ArmouredRegiment> ArmouredRegiments { get; set; } = new List<ArmouredRegiment>();
        public List<ArtilleryRegiment> ArtilleryRegiments { get; set; } = new List<ArtilleryRegiment>();
        public List<CombatEngineeringCompany> Engineers { get; set; } = new List<CombatEngineeringCompany>();
    }

    public class WeaponSystemResult
    {
        public string WeaponType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public double CasualtiesPerWeapon { get; set; }
        public double TotalCasualties { get; set; }
        public int TimeToEngageSeconds { get; set; }
        public double FinalEffectiveness { get; set; }
    }

    public class UnitCombatResult
    {
        public Guid AttackerTokenId { get; set; }
        public Guid DefenderTokenId { get; set; }
        public CombatContext Context { get; set; } = new CombatContext();
        
        public List<WeaponSystemResult> AttackerWeaponResults { get; set; } = new List<WeaponSystemResult>();
        public List<WeaponSystemResult> DefenderWeaponResults { get; set; } = new List<WeaponSystemResult>();
        
        public double TotalAttackerCasualtiesInflicted { get; set; }
        public double TotalDefenderCasualtiesInflicted { get; set; }
        public int TotalEngagementTimeSeconds { get; set; }
        
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    #endregion
}

