using TechWebSol.Models;

namespace TechWebSol.Services
{
    /// <summary>
    /// Comprehensive weapon effectiveness calculator based on NATO doctrine and military field manuals
    /// Sources: FM 3-0 Operations, ATP 3-90.1 Armor and Mechanized Infantry Operations, FM 6-30 Observed Fire
    /// </summary>
    public interface IWeaponEffectivenessService
    {
        WeaponEffectivenessResult CalculateWeaponEffectiveness(WeaponEngagement engagement);
    }

    public class WeaponEffectivenessService : IWeaponEffectivenessService
    {
        /// <summary>
        /// Calculate comprehensive weapon effectiveness including terrain, weather, range, and target factors
        /// </summary>
        public WeaponEffectivenessResult CalculateWeaponEffectiveness(WeaponEngagement engagement)
        {
            var result = new WeaponEffectivenessResult
            {
                WeaponType = engagement.WeaponType,
                TargetType = engagement.TargetType,
                Range = engagement.Range,
                Terrain = engagement.Terrain
            };

            // Base effectiveness by weapon type vs target type
            var baseEffectiveness = GetBaseWeaponEffectiveness(engagement.WeaponType, engagement.TargetType);

            // Apply range modifiers
            var rangeModifier = GetRangeModifier(engagement.WeaponType, engagement.Range);

            // Apply terrain modifiers
            var terrainModifier = GetTerrainModifier(engagement.WeaponType, engagement.Terrain, engagement.TargetType);

            // Apply weather modifiers
            var weatherModifier = GetWeatherModifier(engagement.WeaponType, engagement.Weather);

            // Apply visibility modifiers
            var visibilityModifier = GetVisibilityModifier(engagement.WeaponType, engagement.Visibility);

            // Apply target protection modifiers (fortifications, cover, etc.)
            var protectionModifier = GetProtectionModifier(engagement.WeaponType, engagement.TargetProtection);

            // Apply target posture modifiers (static, moving, suppressed)
            var postureModifier = GetPostureModifier(engagement.WeaponType, engagement.TargetPosture);

            // Calculate final effectiveness
            result.BaseEffectiveness = baseEffectiveness;
            result.RangeModifier = rangeModifier;
            result.TerrainModifier = terrainModifier;
            result.WeatherModifier = weatherModifier;
            result.VisibilityModifier = visibilityModifier;
            result.ProtectionModifier = protectionModifier;
            result.PostureModifier = postureModifier;

            result.FinalEffectiveness = baseEffectiveness 
                * rangeModifier 
                * terrainModifier 
                * weatherModifier 
                * visibilityModifier 
                * protectionModifier 
                * postureModifier;

            // Calculate expected casualties (per weapon)
            result.ExpectedCasualtiesPerWeapon = CalculateExpectedCasualties(engagement.WeaponType, result.FinalEffectiveness);

            // Calculate time to engage
            result.TimeToEngageSeconds = CalculateEngagementTime(engagement.WeaponType, engagement.Terrain, engagement.TargetProtection);

            return result;
        }

        #region Base Weapon Effectiveness (by doctrine)

        /// <summary>
        /// Base weapon effectiveness vs target type
        /// Based on historical data and field manual effectiveness tables
        /// </summary>
        private double GetBaseWeaponEffectiveness(string weaponType, string targetType)
        {
            // Effectiveness scale: 0.0 (no effect) to 1.0 (maximum effect)
            return (weaponType.ToLower(), targetType.ToLower()) switch
            {
                // ATGM effectiveness
                ("atgm", "tank") => 0.85,              // Excellent vs tanks
                ("atgm", "apc") => 0.90,               // Excellent vs light armor
                ("atgm", "infantry") => 0.40,          // Poor vs dispersed infantry
                ("atgm", "artillery") => 0.70,         // Good vs stationary targets
                ("atgm", "fortified") => 0.60,         // Moderate vs fortifications

                // Tank main gun effectiveness
                ("tank", "tank") => 0.75,              // Very good vs tanks
                ("tank", "apc") => 0.85,               // Excellent vs light armor
                ("tank", "infantry") => 0.55,          // Moderate vs infantry
                ("tank", "artillery") => 0.80,         // Excellent vs soft vehicles
                ("tank", "fortified") => 0.45,         // Limited vs fortifications

                // Artillery (155mm HE) effectiveness
                ("artillery", "tank") => 0.25,         // Poor vs armor
                ("artillery", "apc") => 0.40,          // Limited vs light armor
                ("artillery", "infantry") => 0.70,     // Very good vs infantry
                ("artillery", "artillery") => 0.65,    // Good vs soft targets
                ("artillery", "fortified") => 0.50,    // Moderate vs fortifications

                // Heavy MG effectiveness
                ("hmg", "tank") => 0.05,               // Negligible vs armor
                ("hmg", "apc") => 0.15,                // Limited vs light armor
                ("hmg", "infantry") => 0.60,           // Good vs infantry
                ("hmg", "artillery") => 0.35,          // Limited vs vehicles
                ("hmg", "fortified") => 0.20,          // Poor vs fortifications

                // Mortar (120mm HE) effectiveness
                ("mortar", "tank") => 0.10,            // Poor vs armor
                ("mortar", "apc") => 0.25,             // Limited vs light armor
                ("mortar", "infantry") => 0.65,        // Very good vs infantry
                ("mortar", "artillery") => 0.55,       // Good vs soft targets
                ("mortar", "fortified") => 0.35,       // Limited vs fortifications

                // Rocket launcher (RPG type) effectiveness
                ("rocket", "tank") => 0.60,            // Good vs tanks (short range)
                ("rocket", "apc") => 0.75,             // Excellent vs light armor
                ("rocket", "infantry") => 0.35,        // Limited vs infantry
                ("rocket", "artillery") => 0.65,       // Good vs soft vehicles
                ("rocket", "fortified") => 0.50,       // Moderate vs fortifications

                // Infantry rifle/MG effectiveness
                ("infantry", "tank") => 0.02,          // Negligible vs armor
                ("infantry", "apc") => 0.08,           // Very limited vs light armor
                ("infantry", "infantry") => 0.50,      // Moderate vs infantry
                ("infantry", "artillery") => 0.25,     // Limited vs vehicles
                ("infantry", "fortified") => 0.15,     // Poor vs fortifications

                // Grenade launcher effectiveness
                ("grenade", "tank") => 0.05,           // Negligible vs armor
                ("grenade", "apc") => 0.15,            // Limited vs light armor
                ("grenade", "infantry") => 0.55,       // Good vs infantry
                ("grenade", "artillery") => 0.30,      // Limited vs vehicles
                ("grenade", "fortified") => 0.25,      // Limited vs fortifications

                // Default
                _ => 0.30
            };
        }

        #endregion

        #region Range Modifiers

        /// <summary>
        /// Range effectiveness modifier based on weapon optimal ranges
        /// Based on FM 3-21.8 Infantry Rifle Platoon and Squad, ATP 3-90.1
        /// </summary>
        private double GetRangeModifier(string weaponType, double rangeMeters)
        {
            return weaponType.ToLower() switch
            {
                "atgm" => rangeMeters switch
                {
                    < 500 => 0.60,                      // Too close for optimal guidance
                    < 2000 => 1.00,                     // Optimal range
                    < 4000 => 0.90,                     // Good range
                    < 5000 => 0.70,                     // Near max range
                    _ => 0.30                           // Beyond effective range
                },

                "tank" => rangeMeters switch
                {
                    < 500 => 0.95,                      // Point blank
                    < 1500 => 1.00,                     // Optimal range
                    < 2500 => 0.85,                     // Good range
                    < 3500 => 0.60,                     // Long range
                    _ => 0.30                           // Beyond effective range
                },

                "artillery" => rangeMeters switch
                {
                    < 3000 => 0.70,                     // Danger close / minimum range
                    < 10000 => 0.90,                    // Good range
                    < 20000 => 1.00,                    // Optimal range
                    < 30000 => 0.85,                    // Long range
                    _ => 0.50                           // Near max range
                },

                "hmg" => rangeMeters switch
                {
                    < 200 => 0.85,                      // Close range
                    < 800 => 1.00,                      // Optimal range
                    < 1500 => 0.75,                     // Medium range
                    < 2000 => 0.50,                     // Long range
                    _ => 0.20                           // Beyond effective range
                },

                "mortar" => rangeMeters switch
                {
                    < 200 => 0.40,                      // Too close (minimum range)
                    < 1000 => 0.80,                     // Close range
                    < 5000 => 1.00,                     // Optimal range
                    < 7000 => 0.90,                     // Good range
                    _ => 0.60                           // Near max range
                },

                "rocket" => rangeMeters switch
                {
                    < 100 => 0.90,                      // Point blank
                    < 300 => 1.00,                      // Optimal range
                    < 500 => 0.70,                      // Medium range
                    _ => 0.30                           // Beyond effective range
                },

                "infantry" => rangeMeters switch
                {
                    < 100 => 0.95,                      // Close range
                    < 300 => 1.00,                      // Optimal range
                    < 500 => 0.70,                      // Medium range
                    < 800 => 0.40,                      // Long range
                    _ => 0.15                           // Beyond effective range
                },

                "grenade" => rangeMeters switch
                {
                    < 50 => 1.00,                       // Optimal range
                    < 150 => 0.85,                      // Good range
                    < 350 => 0.60,                      // Medium range
                    _ => 0.20                           // Beyond effective range
                },

                _ => 0.70                               // Default
            };
        }

        #endregion

        #region Terrain Modifiers

        /// <summary>
        /// Terrain modifiers based on NATO STANAG 2084 and ATP 3-90.1
        /// Different weapons are affected differently by terrain
        /// </summary>
        private double GetTerrainModifier(string weaponType, string terrain, string targetType)
        {
            return (weaponType.ToLower(), terrain.ToLower()) switch
            {
                // ATGM vs terrain (needs line of sight)
                ("atgm", "open") => 1.00,
                ("atgm", "hills") => 0.90,              // Some LOS issues
                ("atgm", "forest") => 0.50,             // Severe LOS restrictions
                ("atgm", "dense forest") => 0.25,       // Minimal LOS
                ("atgm", "urban") => 0.60,              // Buildings restrict LOS
                ("atgm", "mountains") => 0.70,          // LOS issues but some good positions
                ("atgm", "desert") => 1.00,             // Excellent LOS
                ("atgm", "swamp") => 0.40,              // Poor LOS, vegetation

                // Tank vs terrain
                ("tank", "open") => 1.00,
                ("tank", "hills") => 0.85,              // Reduced mobility
                ("tank", "forest") => 0.60,             // Restricted movement and firing
                ("tank", "dense forest") => 0.35,       // Severely restricted
                ("tank", "urban") => 0.55,              // Vulnerable, restricted movement
                ("tank", "mountains") => 0.50,          // Very restricted mobility
                ("tank", "desert") => 1.00,             // Excellent tank country
                ("tank", "swamp") => 0.30,              // Risk of bogging, poor mobility

                // Artillery vs terrain (indirect fire less affected by terrain)
                ("artillery", "open") => 1.00,
                ("artillery", "hills") => 0.95,         // Slightly harder to range
                ("artillery", "forest") => 0.70,        // Trees reduce effectiveness
                ("artillery", "dense forest") => 0.50,  // Heavy canopy reduces effects
                ("artillery", "urban") => 0.75,         // Buildings absorb effects
                ("artillery", "mountains") => 0.85,     // Good for fire support
                ("artillery", "desert") => 1.00,        // Excellent effectiveness
                ("artillery", "swamp") => 0.80,         // Slightly reduced

                // HMG vs terrain
                ("hmg", "open") => 1.00,
                ("hmg", "hills") => 0.90,
                ("hmg", "forest") => 0.55,              // Trees block fire
                ("hmg", "dense forest") => 0.30,
                ("hmg", "urban") => 0.70,               // Buildings provide cover
                ("hmg", "mountains") => 0.75,
                ("hmg", "desert") => 1.00,
                ("hmg", "swamp") => 0.60,

                // Mortar vs terrain (high angle fire)
                ("mortar", "open") => 1.00,
                ("mortar", "hills") => 1.00,            // High angle negates terrain
                ("mortar", "forest") => 0.75,           // Some tree burst effectiveness
                ("mortar", "dense forest") => 0.60,     // Heavy canopy reduces
                ("mortar", "urban") => 0.80,            // Can fire over buildings
                ("mortar", "mountains") => 0.95,        // Good indirect fire weapon
                ("mortar", "desert") => 1.00,
                ("mortar", "swamp") => 0.85,

                // Rocket launcher vs terrain
                ("rocket", "open") => 1.00,
                ("rocket", "hills") => 0.85,
                ("rocket", "forest") => 0.50,           // Trees block rockets
                ("rocket", "dense forest") => 0.25,
                ("rocket", "urban") => 0.65,            // Good for short range urban
                ("rocket", "mountains") => 0.70,
                ("rocket", "desert") => 1.00,
                ("rocket", "swamp") => 0.45,

                // Infantry weapons vs terrain
                ("infantry", "open") => 0.80,           // Exposed
                ("infantry", "hills") => 0.90,          // Some cover
                ("infantry", "forest") => 1.00,         // Excellent for infantry
                ("infantry", "dense forest") => 0.95,   // Good cover, close range
                ("infantry", "urban") => 1.00,          // Excellent for infantry
                ("infantry", "mountains") => 0.85,
                ("infantry", "desert") => 0.75,         // Limited cover
                ("infantry", "swamp") => 0.70,          // Difficult movement

                // Grenade launcher vs terrain
                ("grenade", "open") => 0.90,
                ("grenade", "hills") => 0.95,
                ("grenade", "forest") => 1.00,          // Excellent vs infantry in forest
                ("grenade", "dense forest") => 1.00,
                ("grenade", "urban") => 1.00,           // Excellent for urban combat
                ("grenade", "mountains") => 0.85,
                ("grenade", "desert") => 0.80,
                ("grenade", "swamp") => 0.75,

                _ => 0.70                               // Default
            };
        }

        #endregion

        #region Weather and Visibility Modifiers

        /// <summary>
        /// Weather effects based on FM 34-81 Weather Support for Army Tactical Operations
        /// </summary>
        private double GetWeatherModifier(string weaponType, string weather)
        {
            return (weaponType.ToLower(), weather.ToLower()) switch
            {
                (_, "clear") => 1.00,
                (_, "overcast") => 0.95,

                // Rain effects
                ("atgm", "light rain") => 0.90,
                ("atgm", "heavy rain") => 0.70,         // Optics degraded
                ("tank", "light rain") => 0.95,
                ("tank", "heavy rain") => 0.85,         // Optics degraded
                ("artillery", "light rain") => 0.95,
                ("artillery", "heavy rain") => 0.90,    // Less affected
                ("infantry", "light rain") => 0.85,
                ("infantry", "heavy rain") => 0.70,     // Vision degraded

                // Fog effects
                ("atgm", "fog") => 0.40,                // Severe LOS restriction
                ("tank", "fog") => 0.50,
                ("artillery", "fog") => 0.80,           // Indirect fire less affected
                ("infantry", "fog") => 0.60,

                // Snow effects
                ("atgm", "snow") => 0.75,
                ("tank", "snow") => 0.70,               // Mobility issues
                ("artillery", "snow") => 0.85,
                ("infantry", "snow") => 0.65,

                // Wind effects (primarily artillery/mortars)
                ("artillery", "high wind") => 0.80,
                ("mortar", "high wind") => 0.75,

                _ => 0.85                               // Default moderate degradation
            };
        }

        /// <summary>
        /// Visibility effects on weapon effectiveness
        /// </summary>
        private double GetVisibilityModifier(string weaponType, string visibility)
        {
            return (weaponType.ToLower(), visibility.ToLower()) switch
            {
                (_, "day clear") => 1.00,
                (_, "day limited") => 0.85,

                // Night operations (with/without thermals)
                ("atgm", "night thermal") => 0.95,      // Modern ATGM have thermal
                ("atgm", "night nods") => 0.70,         // With night vision
                ("atgm", "night naked") => 0.20,        // No night capability
                ("tank", "night thermal") => 0.90,      // Modern tanks have thermals
                ("tank", "night nods") => 0.65,
                ("tank", "night naked") => 0.15,
                ("artillery", "night thermal") => 0.95, // Indirect fire, less affected
                ("artillery", "night nods") => 0.90,
                ("artillery", "night naked") => 0.85,   // Can use indirect with FO
                ("infantry", "night thermal") => 0.80,
                ("infantry", "night nods") => 0.60,
                ("infantry", "night naked") => 0.30,

                // Smoke effects
                ("atgm", "smoke") => 0.30,              // Severe degradation
                ("tank", "smoke") => 0.40,
                ("artillery", "smoke") => 0.85,         // Indirect fire less affected
                ("infantry", "smoke") => 0.35,

                _ => 0.70
            };
        }

        #endregion

        #region Protection and Posture Modifiers

        /// <summary>
        /// Target protection modifiers (cover, fortifications)
        /// Based on FM 5-103 Survivability and ATP 3-37.34 Survivability Operations
        /// </summary>
        private double GetProtectionModifier(string weaponType, string protection)
        {
            return (weaponType.ToLower(), protection.ToLower()) switch
            {
                // Open/no protection
                (_, "none") => 1.00,
                (_, "open") => 1.00,

                // Hasty fighting positions
                ("artillery", "hasty") => 0.85,
                ("mortar", "hasty") => 0.85,
                ("tank", "hasty") => 0.90,
                (_, "hasty") => 0.80,                   // Infantry weapons

                // Improved fighting positions
                ("artillery", "improved") => 0.70,
                ("mortar", "improved") => 0.70,
                ("atgm", "improved") => 0.75,
                ("tank", "improved") => 0.80,
                (_, "improved") => 0.60,

                // Fortified positions
                ("artillery", "fortified") => 0.55,
                ("mortar", "fortified") => 0.55,
                ("atgm", "fortified") => 0.60,
                ("tank", "fortified") => 0.70,
                (_, "fortified") => 0.45,

                // Bunkers/heavy fortifications
                ("artillery", "bunker") => 0.40,
                ("mortar", "bunker") => 0.40,
                ("atgm", "bunker") => 0.50,
                ("tank", "bunker") => 0.60,
                (_, "bunker") => 0.30,

                // Urban cover
                ("artillery", "urban") => 0.65,
                ("mortar", "urban") => 0.65,
                (_, "urban") => 0.70,

                _ => 0.75
            };
        }

        /// <summary>
        /// Target posture modifiers (moving, static, suppressed)
        /// </summary>
        private double GetPostureModifier(string weaponType, string posture)
        {
            return posture.ToLower() switch
            {
                "static" => 1.00,
                "moving slow" => 0.80,
                "moving fast" => 0.60,
                "suppressed" => 1.20,                   // Suppressed targets easier to hit
                "counterfire" => 0.70,                  // Target shooting back
                _ => 0.85
            };
        }

        #endregion

        #region Casualty and Time Calculations

        /// <summary>
        /// Calculate expected casualties per weapon based on effectiveness
        /// </summary>
        private double CalculateExpectedCasualties(string weaponType, double finalEffectiveness)
        {
            // Base casualties per weapon per engagement
            var baseCasualties = weaponType.ToLower() switch
            {
                "atgm" => 1.0,                          // 1 kill per shot on average
                "tank" => 0.8,                          // 0.8 kills per shot
                "artillery" => 0.3,                     // 0.3 kills per round (area effect)
                "hmg" => 0.05,                          // 0.05 kills per burst
                "mortar" => 0.2,                        // 0.2 kills per round
                "rocket" => 0.7,                        // 0.7 kills per shot
                "infantry" => 0.02,                     // 0.02 kills per shot (rifle)
                "grenade" => 0.15,                      // 0.15 kills per grenade
                _ => 0.1
            };

            return baseCasualties * finalEffectiveness;
        }

        /// <summary>
        /// Calculate time to engage target (seconds)
        /// </summary>
        private int CalculateEngagementTime(string weaponType, string terrain, string protection)
        {
            var baseTime = weaponType.ToLower() switch
            {
                "atgm" => 30,                           // 30 seconds to acquire and fire
                "tank" => 10,                           // 10 seconds
                "artillery" => 120,                     // 2 minutes (call for fire)
                "hmg" => 5,                             // 5 seconds
                "mortar" => 60,                         // 1 minute
                "rocket" => 15,                         // 15 seconds
                "infantry" => 2,                        // 2 seconds
                "grenade" => 3,                         // 3 seconds
                _ => 10
            };

            // Adjust for terrain difficulty
            var terrainMultiplier = terrain.ToLower() switch
            {
                "forest" => 1.5,
                "dense forest" => 2.0,
                "urban" => 1.3,
                "mountains" => 1.4,
                _ => 1.0
            };

            // Adjust for target protection (harder to acquire)
            var protectionMultiplier = protection.ToLower() switch
            {
                "fortified" => 1.5,
                "bunker" => 2.0,
                "improved" => 1.3,
                _ => 1.0
            };

            return (int)(baseTime * terrainMultiplier * protectionMultiplier);
        }

        #endregion
    }

    #region Data Models

    public class WeaponEngagement
    {
        public string WeaponType { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public double Range { get; set; }
        public string Terrain { get; set; } = "open";
        public string Weather { get; set; } = "clear";
        public string Visibility { get; set; } = "day clear";
        public string TargetProtection { get; set; } = "none";
        public string TargetPosture { get; set; } = "static";
    }

    public class WeaponEffectivenessResult
    {
        public string WeaponType { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public double Range { get; set; }
        public string Terrain { get; set; } = string.Empty;
        
        public double BaseEffectiveness { get; set; }
        public double RangeModifier { get; set; }
        public double TerrainModifier { get; set; }
        public double WeatherModifier { get; set; }
        public double VisibilityModifier { get; set; }
        public double ProtectionModifier { get; set; }
        public double PostureModifier { get; set; }
        
        public double FinalEffectiveness { get; set; }
        public double ExpectedCasualtiesPerWeapon { get; set; }
        public int TimeToEngageSeconds { get; set; }
    }

    #endregion
}

