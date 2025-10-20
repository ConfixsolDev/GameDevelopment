using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;
using TechWebSol.Services.MapManagement;
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
        /// Set the current terrain database path in session
        /// </summary>
        [HttpPost]
        public IActionResult SetTerrainDatabase([FromBody] SetTerrainDatabaseRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.TerrainDbPath))
                {
                    return BadRequest(new { success = false, message = "Terrain database path is required" });
                }
                
                // Validate that the terrain database exists
                var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var normalizedPath = request.TerrainDbPath.Replace('/', Path.DirectorySeparatorChar);
                var dbPath = Path.Combine(wwwRoot, normalizedPath);
                
                if (!System.IO.File.Exists(dbPath))
                {
                    _logger.LogWarning("Terrain database not found at path: {Path}", dbPath);
                    return NotFound(new { success = false, message = "Terrain database not found" });
                }
                
                // Store in session
                HttpContext.Session.SetString("CurrentTerrainDb", request.TerrainDbPath);
                
                _logger.LogInformation("Terrain database set in session: {Path}", request.TerrainDbPath);
                
                return Json(new { success = true, message = "Terrain database set successfully", path = request.TerrainDbPath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting terrain database");
                return StatusCode(500, new { success = false, message = "Error setting terrain database" });
            }
        }

        /// <summary>
        /// Test endpoint to verify routing is working
        /// </summary>
        [HttpGet]
        public IActionResult TestApi()
        {
            return Json(new { success = true, message = "API routing is working", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Get current team information for force-based color coding
        /// </summary>
        [HttpGet]
        public IActionResult GetCurrentTeamInfo()
        {
            try
            {
                // Get fresh user data to ensure ForceType is current
                var currentUser = _userSessionService.GetCurrentUser();
                
                if (currentUser?.TeamId == null)
                {
                    return Json(new { success = false, message = "No team assigned to current user" });
                }

                var team = _context.Teams.FirstOrDefault(t => t.Id == currentUser.TeamId);
                
                if (team == null)
                {
                    return Json(new { success = false, message = "Team not found" });
                }

                // Determine force type with fallback logic
                var forceType = currentUser.ForceType ?? team.ForceType ?? "Neutral";
                
                _logger.LogInformation($"Force type determined: {forceType} (User: {currentUser.ForceType}, Team: {team.ForceType})");

                var result = new
                {
                    success = true,
                    team = new
                    {
                        id = team.Id,
                        name = team.Name,
                        forceType = forceType
                    }
                };

                _logger.LogInformation($"Returning team info: {result.team.name} with force type: {result.team.forceType}");
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current team info");
                return Json(new { success = false, message = "Error retrieving team information" });
            }
        }

        /// <summary>
        /// Military Adjudication - Comprehensive analysis of all token movements
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AdjudicateAllMovements()
        {
            try
            {
                // Get all placed tokens for the user's team
                var tokens = await _context.Tokens
                    .Include(t => t.TokenGroup)
                    .Include(t => t.MapMarkers.OrderBy(m => m.CreatedDate))
                    .Where(t => t.TeamId == user.TeamId && t.MapMarkers.Any())
                    .ToListAsync();

                // Get all military units for quick lookup (using all unit types)
                var infantryUnits = await _context.InfantryBattalions
       .Where(mu => mu.TeamId == user.TeamId && mu.TokenId != null)
       .GroupBy(mu => mu.TokenId.Value)
       .Select(g => g.First())
       .ToDictionaryAsync(mu => mu.TokenId.Value, mu => (MilitaryUnit)mu);

                var armouredUnits = await _context.ArmouredRegiments
                    .Where(mu => mu.TeamId == user.TeamId && mu.TokenId != null)
                    .ToDictionaryAsync(mu => mu.TokenId.Value, mu => (MilitaryUnit)mu);

                var artilleryUnits = await _context.ArtilleryRegiments
                    .Where(mu => mu.TeamId == user.TeamId && mu.TokenId != null)
                    .ToDictionaryAsync(mu => mu.TokenId.Value, mu => (MilitaryUnit)mu);

                var logisticsUnits = await _context.LogisticsUnits
                    .Where(mu => mu.TeamId == user.TeamId && mu.TokenId != null)
                    .ToDictionaryAsync(mu => mu.TokenId.Value, mu => (MilitaryUnit)mu);

                var engineeringUnits = await _context.CombatEngineeringCompanies
                    .Where(mu => mu.TeamId == user.TeamId && mu.TokenId != null)
                    .ToDictionaryAsync(mu => mu.TokenId.Value, mu => (MilitaryUnit)mu);

                // Merge all units into one dictionary
                var militaryUnits = new Dictionary<Guid, MilitaryUnit>();
                foreach (var unit in infantryUnits) militaryUnits[unit.Key] = unit.Value;
                foreach (var unit in armouredUnits) militaryUnits[unit.Key] = unit.Value;
                foreach (var unit in artilleryUnits) militaryUnits[unit.Key] = unit.Value;
                foreach (var unit in logisticsUnits) militaryUnits[unit.Key] = unit.Value;
                foreach (var unit in engineeringUnits) militaryUnits[unit.Key] = unit.Value;

                var adjudicationResults = new List<object>();

                foreach (var token in tokens)
                {
                    try
                    {
                        _logger.LogInformation($"Processing token: {token.Id}");

                        var markers = token.MapMarkers.OrderBy(m => m.CreatedDate).ToList();

                        if (markers.Count < 2)
                            continue; // Need at least 2 points for movement analysis

                        // Calculate total distance
                        double totalDistance = 0;
                        var segments = new List<object>();

                        try
                        {
                            _logger.LogInformation($"Calculating distances for {markers.Count} markers");

                            for (int i = 0; i < markers.Count - 1; i++)
                            {
                                var start = markers[i];
                                var end = markers[i + 1];

                                var distance = CalculateDistance(
                                    double.Parse(start.latitude), double.Parse(start.longitude),
                                    double.Parse(end.latitude), double.Parse(end.longitude)
                                );

                                totalDistance += distance;

                                // Determine terrain type and MP modifier
                                var terrainType = DetermineTerrainType(start, end);
                                var mpModifier = GetMPModifier(terrainType, GetMobilityType(token));
                                var segmentMP = (int)Math.Round(distance * mpModifier);

                                segments.Add(new
                                {
                                    segmentNumber = i + 1,
                                    distance = Math.Round(distance, 2),
                                    terrainType = terrainType,
                                    mpModifier = mpModifier,
                                    mpConsumption = segmentMP,
                                    from = new { lat = double.Parse(start.latitude), lng = double.Parse(start.longitude) },
                                    to = new { lat = double.Parse(end.latitude), lng = double.Parse(end.longitude) }
                                });
                            }

                            _logger.LogInformation($"Getting unit data for token {token.Id}");

                            militaryUnits.TryGetValue(token.Id, out var militaryUnit);
                            var unitType = token.TokenGroup?.Name ?? "Infantry";
                            var mobilityType = GetMobilityType(token);
                            var baseMovementPoints = GetBaseMovementPoints(mobilityType);

                            _logger.LogInformation($"Calculating MP consumption for token {token.Id}");

                            var totalMP = (int)segments.Sum(s => (int)((dynamic)s).mpConsumption);
                            var mpUtilization = (double)totalMP / baseMovementPoints;

                            _logger.LogInformation($"Total MP: {totalMP}, MP Utilization: {mpUtilization}");

                            _logger.LogInformation($"Determining feasibility for token {token.Id}");
                            var terrainAnalysis = await GetTerrainAnalysis(markers, mobilityType, militaryUnit);

                            // Get terrain blockage status
                            var terrainAnalysisDynamic = terrainAnalysis as dynamic;
                            var isRouteBlocked = false;
                            var blockageReason = "";
                            
                            try
                            {
                                if (terrainAnalysisDynamic != null)
                                {
                                    isRouteBlocked = (bool)(terrainAnalysisDynamic.isRouteBlocked ?? false);
                                    blockageReason = (string)(terrainAnalysisDynamic.blockageReason ?? "");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Error accessing terrain blockage status");
                            }

                            // Route is NOT feasible if terrain blocks it OR if MP is insufficient
                            var isFeasible = !isRouteBlocked && totalMP <= (int)baseMovementPoints;
                            var feasibilityStatus = isRouteBlocked 
                                ? "terrain_blocked" 
                                : DetermineFeasibilityStatus(mpUtilization, totalMP, baseMovementPoints);


                            _logger.LogInformation($"Calculating time estimate for token {token.Id}");

                            var baseSpeed = GetBaseSpeed(mobilityType);
                            var estimatedHours = totalDistance / baseSpeed;

                            _logger.LogInformation($"Generating recommendations for token {token.Id}");

                            var recommendations = GenerateRecommendations(
                                feasibilityStatus,
                                mpUtilization,
                                totalDistance,
                                segments.Cast<dynamic>().ToList()
                            );

                            _logger.LogInformation($"Generated {recommendations.Count} recommendations");
                       
                            _logger.LogInformation($"Creating result object for token {token.Id}");

                             // Get detailed unit composition
                             var unitComposition = GetUnitComposition(militaryUnit);
                             
                             // Get terrain analysis with mobility type and unit for proper passability checks
                             
                             // Calculate realistic time based on terrain and unit capabilities
                             var realisticTimeEstimate = CalculateRealisticTimeEstimate(
                                 totalDistance, mobilityType, militaryUnit, terrainAnalysis);

                             // Extract realistic time values safely
                             var realisticHours = 0;
                             var realisticMinutes = 0;
                             var realisticTotalHours = 0.0;
                             var effectiveSpeed = 0.0;

                             try
                             {
                                 var dynEstimate = realisticTimeEstimate as dynamic;
                                 if (dynEstimate != null)
                                 {
                                     realisticHours = (int)(dynEstimate.hours ?? 0);
                                     realisticMinutes = (int)(dynEstimate.minutes ?? 0);
                                     realisticTotalHours = (double)(dynEstimate.totalHours ?? 0.0);
                                     effectiveSpeed = (double)(dynEstimate.effectiveSpeed ?? 0.0);
                                 }
                             }
                             catch (Exception ex)
                             {
                                 _logger.LogWarning(ex, "Error extracting realistic time estimates");
                             }

                             adjudicationResults.Add(new
                             {
                                 tokenId = token.Id,
                                 tokenName = token.Name,
                                 unitType = unitType,
                                 forceType = token.ForceType,
                                 mobilityType = mobilityType,
                                 strength = militaryUnit?.StrengthPercentage ?? 100,
                                 supplyState = GetSupplyStateText(militaryUnit?.SupplyState ?? 100),
                                 combatPower = militaryUnit?.CombatPower ?? 0,

                                 // Detailed Unit Composition
                                 unitComposition = unitComposition,

                                 movement = new
                                 {
                                     totalDistance = Math.Round(totalDistance, 2),
                                     waypointCount = markers.Count,
                                     segments = segments
                                 },

                                 mpAnalysis = new
                                 {
                                     baseMovementPoints = baseMovementPoints,
                                     totalMPRequired = totalMP,
                                     mpUtilization = Math.Round(mpUtilization * 100, 1),
                                     remainingMP = Math.Max(0, (int)(baseMovementPoints - totalMP))
                                 },

                                 feasibility = new
                                 {
                                     isFeasible = isFeasible,
                                     status = feasibilityStatus,
                                     reason = isRouteBlocked 
                                         ? blockageReason 
                                         : GetFeasibilityReason(feasibilityStatus, totalMP, baseMovementPoints),
                                     isRouteBlocked = isRouteBlocked,
                                     terrainBlocked = isRouteBlocked
                                 },

                                 timeEstimate = new
                                 {
                                     hours = (int)estimatedHours,
                                     minutes = (int)((estimatedHours - (int)estimatedHours) * 60),
                                     totalHours = Math.Round(estimatedHours, 2),
                                     realisticHours = realisticHours,
                                     realisticMinutes = realisticMinutes,
                                     realisticTotalHours = Math.Round(realisticTotalHours, 2),
                                     effectiveSpeed = Math.Round(effectiveSpeed, 1)
                                 },

                                 // Enhanced Terrain Analysis
                                 terrainAnalysis = terrainAnalysis,

                                 recommendations = recommendations,

                                 waypoints = markers.Select(m => new
                                 {
                                     lat = m.latitude,
                                     lng = m.longitude,
                                     timestamp = m.CreatedDate
                                 }).ToList()
                             });

                            _logger.LogInformation($"Successfully created result for token {token.Id}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error creating result object for token {token.Id}: {ex.Message}");
                            throw;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing token {token.Id}: {ex.Message}");
                        // Continue with next token instead of failing completely
                        continue;
                    }
                }

                 double totalTokens = adjudicationResults.Count;
                 double feasibleCount = adjudicationResults.Count(r => (bool)((dynamic)r).feasibility.isFeasible);
                 double blockedCount = adjudicationResults.Count(r => !(bool)((dynamic)r).feasibility.isFeasible);
                 double totalDistance1 = adjudicationResults.Sum(r => (double)((dynamic)r).movement.totalDistance);
                 double avgMPUtilization = adjudicationResults.Any() ?
                         adjudicationResults.Average(r => (double)((dynamic)r).mpAnalysis.mpUtilization) : 0;

                var summary = new 
                {
                    totalTokens,
                    feasibleCount,
                    blockedCount,
                    totalDistance= totalDistance1,
                    avgMPUtilization
                };

                return Json(new
                {
                    success = true,
                    summary = summary,
                    results = adjudicationResults,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in military adjudication");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper methods for military adjudication
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in km
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private string DetermineTerrainType(MapMarker start, MapMarker end)
        {
            // Simplified terrain determination - can be enhanced with actual terrain data
            return "cross_country"; // Default
        }

        private string GetMobilityType(Token token)
        {
            var unitType = token.TokenGroup?.Name?.ToLower() ?? "";

            if (unitType.Contains("armour") || unitType.Contains("tank") || unitType.Contains("mechanized"))
                return "tracked";
            if (unitType.Contains("artillery") || unitType.Contains("logistics"))
                return "wheeled";

            return "foot";
        }

        private double GetMPModifier(string terrainType, string mobilityType)
        {
            var modifiers = new Dictionary<string, Dictionary<string, double>>
            {
                ["road"] = new() { ["wheeled"] = 1.0, ["tracked"] = 1.0, ["foot"] = 1.5 },
                ["cross_country"] = new() { ["wheeled"] = 2.0, ["tracked"] = 1.5, ["foot"] = 1.0 },
                ["rough"] = new() { ["wheeled"] = 3.0, ["tracked"] = 2.0, ["foot"] = 1.5 },
                ["forest"] = new() { ["wheeled"] = 4.0, ["tracked"] = 2.5, ["foot"] = 1.2 },
                ["water"] = new() { ["wheeled"] = 999.0, ["tracked"] = 999.0, ["foot"] = 999.0 }
            };

            if (modifiers.ContainsKey(terrainType) && modifiers[terrainType].ContainsKey(mobilityType))
                return modifiers[terrainType][mobilityType];

            return 2.0; // Default modifier
        }

        private double GetBaseMovementPoints(string mobilityType)
        {
            return mobilityType switch
            {
                "foot" => 30.0,
                "wheeled" => 40.0,
                "tracked" => 35.0,
                _ => 30.0
            };
        }

        private double GetBaseSpeed(string mobilityType)
        {
            return mobilityType switch
            {
                "foot" => 5.0,      // 5 km/h
                "wheeled" => 30.0,  // 30 km/h
                "tracked" => 25.0,  // 25 km/h
                _ => 5.0
            };
        }

        private string DetermineFeasibilityStatus(double mpUtilization, int totalMP, double baseMP)
        {
            if (totalMP > baseMP)
                return "insufficient_mp";
            if (mpUtilization > 0.8)
                return "high_consumption";
            if (mpUtilization > 0.5)
                return "moderate";

            return "feasible";
        }

        private string GetFeasibilityReason(string status, int totalMP, double baseMP)
        {
            return status switch
            {
                "terrain_blocked" => "Route BLOCKED by impassable terrain obstacles",
                "insufficient_mp" => $"Insufficient movement points ({totalMP}/{baseMP})",
                "high_consumption" => $"High MP consumption ({totalMP}/{baseMP})",
                "moderate" => $"Moderate MP usage ({totalMP}/{baseMP})",
                "feasible" => $"Route is feasible ({totalMP}/{baseMP})",
                _ => "Unknown status"
            };
        }

        private List<object> GenerateRecommendations(string status, double mpUtilization, double totalDistance, List<dynamic> segments)
        {
            var recommendations = new List<object>();

            // Critical: Terrain blockage
            if (status == "terrain_blocked")
            {
                recommendations.Add(new
                {
                    priority = "high",
                    type = "terrain",
                    message = "⛔ ROUTE IMPASSABLE - Terrain obstacles block movement. Select an alternate route avoiding water, cliffs, or steep terrain."
                });
                recommendations.Add(new
                {
                    priority = "high",
                    type = "planning",
                    message = "Consider engineering support (bridge-laying, road construction) or air/water transport alternatives"
                });
                // Return immediately - other recommendations are irrelevant if route is blocked
                return recommendations;
            }

            if (status == "insufficient_mp")
            {
                recommendations.Add(new
                {
                    priority = "high",
                    type = "movement",
                    message = "Consider reducing route length or requesting additional movement points"
                });
            }

            if (mpUtilization > 0.8)
            {
                recommendations.Add(new
                {
                    priority = "medium",
                    type = "planning",
                    message = "Plan for rest stops or supply points along the route"
                });
            }

            if (totalDistance > 50)
            {
                recommendations.Add(new
                {
                    priority = "medium",
                    type = "logistics",
                    message = "Long distance movement - ensure adequate fuel and supplies"
                });
            }

            // Check for difficult terrain segments
            var difficultSegments = segments.Where(s => s.mpModifier >= 3.0).ToList();
            if (difficultSegments.Any())
            {
                recommendations.Add(new
                {
                    priority = "medium",
                    type = "terrain",
                    message = $"Route contains {difficultSegments.Count} difficult terrain segment(s) - expect slower progress"
                });
            }

            if (!recommendations.Any())
            {
                recommendations.Add(new
                {
                    priority = "low",
                    type = "status",
                    message = "Movement plan appears sound - proceed as planned"
                });
            }

            return recommendations;
        }

        private string GetSupplyStateText(int supplyState)
        {
            return supplyState switch
            {
                >= 100 => "Green",
                >= 75 => "Amber", 
                >= 50 => "Red",
                _ => "Critical"
            };
        }

        private object GetUnitComposition(MilitaryUnit militaryUnit)
        {
            if (militaryUnit == null)
            {
                return new
                {
                    personnel = 0,
                    vehicles = 0,
                    weapons = 0,
                    details = "No unit data available"
                };
            }

            return militaryUnit switch
            {
                InfantryBattalion infantry => new
                {
                    personnel = infantry.Strength,
                    vehicles = 0, // Infantry typically on foot
                    weapons = infantry.ATGMS + infantry.RocketLauncher + infantry.Mortars81mm + 
                             infantry.Mortars120mm + infantry.GrenadeLaunchers + infantry.HMG_AGL + 
                             infantry.MG_LMG + infantry.MANPADS,
                    details = $"Infantry Battalion: {infantry.Companies} companies, " +
                             $"{infantry.ATGMS} ATGMs, {infantry.Mortars120mm} 120mm mortars, " +
                             $"{infantry.HMG_AGL} HMGs, {infantry.Drones} drones",
                    companies = infantry.Companies,
                    atgms = infantry.ATGMS,
                    mortars = infantry.Mortars120mm,
                    hmgs = infantry.HMG_AGL,
                    drones = infantry.Drones
                },
                ArmouredRegiment armour => new
                {
                    personnel = armour.Strength,
                    vehicles = armour.Tanks,
                    weapons = armour.Tanks + armour.ATGMS + armour.Mortars120mm + armour.HMG,
                    details = $"Armoured Regiment: {armour.Squadrons} squadrons, " +
                             $"{armour.Tanks} tanks, {armour.ATGMS} ATGMs, " +
                             $"{armour.HMG} HMGs, {armour.Drones} drones",
                    squadrons = armour.Squadrons,
                    tanks = armour.Tanks,
                    atgms = armour.ATGMS,
                    hmgs = armour.HMG,
                    drones = armour.Drones
                },
                ArtilleryRegiment artillery => new
                {
                    personnel = artillery.Strength,
                    vehicles = artillery.Guns,
                    weapons = artillery.Guns + artillery.HMG,
                    details = $"Artillery Regiment: {artillery.Batteries} batteries, " +
                             $"{artillery.Guns} {artillery.GunCaliber} guns, " +
                             $"range {artillery.GunRange}km, {artillery.Drones} drones",
                    batteries = artillery.Batteries,
                    guns = artillery.Guns,
                    gunCaliber = artillery.GunCaliber,
                    gunRange = artillery.GunRange,
                    hmgs = artillery.HMG,
                    drones = artillery.Drones
                },
                LogisticsUnit logistics => new
                {
                    personnel = logistics.Strength,
                    vehicles = logistics.SupplyTrucks + logistics.FuelTrucks + logistics.WaterTrucks + 
                              logistics.AmmunitionTrucks + logistics.MaintenanceVehicles + 
                              logistics.RecoveryVehicles + logistics.MobileWorkshops,
                    weapons = logistics.HMG + logistics.LMG,
                    details = $"Logistics Unit: {logistics.Companies} companies, " +
                             $"{logistics.SupplyTrucks} supply trucks, {logistics.FuelTrucks} fuel trucks, " +
                             $"{logistics.AmmunitionTrucks} ammo trucks, {logistics.FuelCapacity}L fuel capacity",
                    companies = logistics.Companies,
                    supplyTrucks = logistics.SupplyTrucks,
                    fuelTrucks = logistics.FuelTrucks,
                    ammoTrucks = logistics.AmmunitionTrucks,
                    fuelCapacity = logistics.FuelCapacity
                },
                CombatEngineeringCompany engineers => new
                {
                    personnel = engineers.Strength,
                    vehicles = engineers.EngineerVehicles + engineers.BridgeLayingVehicles + 
                              engineers.MineClearingVehicles + engineers.Bulldozers + 
                              engineers.Excavators + engineers.Cranes,
                    weapons = engineers.HMG + engineers.LMG + engineers.ATGMS,
                    details = $"Combat Engineering Company: {engineers.Platoons} platoons, " +
                             $"{engineers.EngineerVehicles} engineer vehicles, " +
                             $"{engineers.BridgeLayingVehicles} bridge layers, " +
                             $"{engineers.MineClearingVehicles} mine clearers",
                    platoons = engineers.Platoons,
                    engineerVehicles = engineers.EngineerVehicles,
                    bridgeLayers = engineers.BridgeLayingVehicles,
                    mineClearers = engineers.MineClearingVehicles,
                    bulldozers = engineers.Bulldozers
                },
                _ => new
                {
                    personnel = militaryUnit.Strength,
                    vehicles = 0,
                    weapons = 0,
                    details = $"Generic Unit: {militaryUnit.Name}"
                }
            };
        }

        private async Task<object> GetTerrainAnalysis(List<MapMarker> markers, string mobilityType = "tracked", MilitaryUnit militaryUnit = null)
        {
            try
            {
                var startElevation = 0.0;
                var endElevation = 0.0;
                var maxSlope = 0.0;
                var obstacles = new List<object>();
                var elevations = new List<double>();
                
                // Categorized obstacle counters (like TacticalViewer)
                var obstacleCategories = new Dictionary<string, int>
                {
                    ["water"] = 0,
                    ["cliff"] = 0,
                    ["wetland"] = 0,
                    ["steep"] = 0,
                    ["forest"] = 0,
                    ["urban"] = 0,
                    ["desert"] = 0,
                    ["military"] = 0
                };

                // Get vehicle weight for passability checks (default to 30 tons if not specified)
                var vehicleWeight = 30.0; // Default for tracked vehicles
                if (militaryUnit != null)
                {
                    // Estimate vehicle weight based on unit type
                    if (militaryUnit is ArmouredRegiment armoured)
                        vehicleWeight = 40.0; // Tanks are heavy
                    else if (militaryUnit is ArtilleryRegiment)
                        vehicleWeight = 25.0; // Artillery vehicles
                    else if (militaryUnit is LogisticsUnit)
                        vehicleWeight = 20.0; // Supply trucks
                    else if (militaryUnit is InfantryBattalion)
                        vehicleWeight = 15.0; // Light vehicles/foot
                    else if (militaryUnit is CombatEngineeringCompany)
                        vehicleWeight = 35.0; // Heavy engineer vehicles
                }

                // Get real elevation data for all markers
                if (markers.Count >= 2)
                {
                    try
                    {
                        // Get current terrain database from header, session, or null
                        var terrainDb = Request.Headers["X-Terrain-Database"].FirstOrDefault() 
                                        ?? HttpContext.Session.GetString("CurrentTerrainDb") 
                                        ?? null;
                        
                        _logger.LogInformation($"Terrain analysis - Header: {Request.Headers["X-Terrain-Database"].FirstOrDefault()}, Session: {HttpContext.Session.GetString("CurrentTerrainDb")}, Final: {terrainDb}");
                        
                        if (string.IsNullOrEmpty(terrainDb))
                        {
                            _logger.LogWarning("No terrain database specified for terrain analysis");
                            // Skip elevation lookup - will use fallback analysis below
                        }
                        else
                        {
                        // Build detailed route points with intermediate sampling (like TacticalViewer)
                        var detailedPoints = new List<object>();
                        
                        for (int i = 0; i < markers.Count - 1; i++)
                        {
                            var startLat = double.Parse(markers[i].latitude);
                            var startLng = double.Parse(markers[i].longitude);
                            var endLat = double.Parse(markers[i + 1].latitude);
                            var endLng = double.Parse(markers[i + 1].longitude);
                            
                            // Calculate distance between waypoints
                            var distance = CalculateDistance(startLat, startLng, endLat, endLng);
                            
                            // Determine number of segments (adaptive ~100m spacing, min 10, max 300)
                            var segments = Math.Max(10, Math.Min(300, (int)Math.Floor(distance / 0.1))); // 0.1km = 100m
                            
                            // Add intermediate points
                            for (int j = 0; j <= segments; j++)
                            {
                                var ratio = (double)j / segments;
                                detailedPoints.Add(new
                                {
                                    latitude = startLat + (endLat - startLat) * ratio,
                                    longitude = startLng + (endLng - startLng) * ratio
                                });
                            }
                        }
                        
                        _logger.LogInformation($"Sampling {detailedPoints.Count} points along route for terrain analysis");
                        
                        // Create elevation lookup request with detailed points
                        var elevationRequest = new
                        {
                            locations = detailedPoints
                        };

                        // Call elevation lookup API
                        _logger.LogInformation($"Calling elevation lookup API with {detailedPoints.Count} points, terrainDb: {terrainDb}");
                        var elevationResponse = await CallElevationLookup(elevationRequest, terrainDb);
                        
                        _logger.LogInformation($"Elevation lookup response: success={elevationResponse?.success}, results count={elevationResponse?.results?.Count ?? 0}");
                        
                        if (elevationResponse != null && elevationResponse.success == true && elevationResponse.results != null)
                        {
                            var resultsList = elevationResponse.results as IEnumerable<dynamic>;
                            if (resultsList != null)
                            {
                                elevations = resultsList.Select(r => (double)r.elevation).ToList();
                            }
                            
                            if (elevations.Count > 0)
                            {
                                startElevation = elevations[0];
                                endElevation = elevations[elevations.Count - 1];
                                
                                // Calculate max slope along route and check for impassable slopes
                                // Like TacticalViewer: Heavy vehicles struggle on slopes > 25°, trucks > 35°
                                var slopeLimit = vehicleWeight > 20 ? 25.0 : 35.0;
                                
                                // Calculate slopes between consecutive detailed points
                                for (int i = 0; i < elevations.Count - 1; i++)
                                {
                                    // Calculate distance between consecutive detailed points (~100m spacing)
                                    var pointDistance = 0.1; // 100m in km
                                    
                                    if (pointDistance > 0)
                                    {
                                        var elevationDiff = Math.Abs(elevations[i + 1] - elevations[i]);
                                        var slope = Math.Atan(elevationDiff / (pointDistance * 1000)) * 180 / Math.PI;
                                        maxSlope = Math.Max(maxSlope, slope);
                                        
                                        // Check if slope is impassable for this vehicle
                                        if (slope > slopeLimit)
                                        {
                                            obstacleCategories["steep"]++;
                                            obstacles.Add(new { 
                                                type = "steep_terrain",
                                                isImpassable = true,
                                                description = $"⛰️ STEEP TERRAIN: {Math.Round(slope, 1)}° slope - IMPASSABLE for {vehicleWeight}T vehicle (limit: {slopeLimit}°)"
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get real elevation data, using fallback");
                    }
                }

                // Get terrain features along the route
                if (markers.Count >= 2)
                {
                    try
                    {
                        var terrainDb = Request.Headers["X-Terrain-Database"].FirstOrDefault() 
                                        ?? HttpContext.Session.GetString("CurrentTerrainDb") 
                                        ?? null;
                        
                        if (string.IsNullOrEmpty(terrainDb))
                        {
                            _logger.LogWarning("No terrain database specified for terrain features");
                            // Skip terrain features lookup - will use fallback analysis below
                        }
                        else
                        {
                        // Calculate bounding box for the detailed route (more accurate)
                        var lats = markers.Select(m => double.Parse(m.latitude)).ToList();
                        var lngs = markers.Select(m => double.Parse(m.longitude)).ToList();
                        
                        // Add buffer for detailed sampling
                        var buffer = 0.005; // 500m buffer
                        var bbox = new double[] {
                            lngs.Min() - buffer, // minLng
                            lats.Min() - buffer, // minLat  
                            lngs.Max() + buffer, // maxLng
                            lats.Max() + buffer  // maxLat
                        };

                        // Get terrain features
                        var featuresResponse = await CallTerrainFeatures(bbox, terrainDb);
                        
                        if (featuresResponse != null && featuresResponse.success == true && featuresResponse.elements != null)
                        {
                            var elementsList = featuresResponse.elements as IEnumerable<dynamic>;
                            if (elementsList != null)
                            {
                                // Analyze features for obstacles (like TacticalViewer)
                                foreach (var element in elementsList)
                                {
                                    if (element.tags != null)
                                    {
                                        var tags = element.tags as Dictionary<string, string>;
                                        if (tags != null)
                                        {
                                            // Check for water features - IMPASSABLE
                                            if (tags.ContainsKey("natural") && tags["natural"] == "water" ||
                                                tags.ContainsKey("waterway") ||
                                                tags.ContainsKey("water"))
                                            {
                                                obstacleCategories["water"]++;
                                                obstacles.Add(new { 
                                                    type = "water",
                                                    isImpassable = true,
                                                    description = $"🌊 WATER: {tags.GetValueOrDefault("name", "Water crossing")} - IMPASSABLE (requires bridge/boat)"
                                                });
                                            }
                                            
                                            // Check for cliffs/rock - IMPASSABLE
                                            if (tags.ContainsKey("natural") && 
                                                (tags["natural"] == "cliff" || tags["natural"] == "rock"))
                                            {
                                                obstacleCategories["cliff"]++;
                                                obstacles.Add(new { 
                                                    type = "cliff",
                                                    isImpassable = true,
                                                    description = $"🪨 CLIFF/ROCK: {tags.GetValueOrDefault("name", "Rock face")} - IMPASSABLE"
                                                });
                                            }
                                            
                                            // Check for wetland - IMPASSABLE for heavy vehicles
                                            if (tags.ContainsKey("natural") && tags["natural"] == "wetland")
                                            {
                                                obstacleCategories["wetland"]++;
                                                var isImpassable = vehicleWeight > 30;
                                                obstacles.Add(new { 
                                                    type = "wetland",
                                                    isImpassable = isImpassable,
                                                    description = isImpassable 
                                                        ? $"💧 WETLAND: Vehicle too heavy ({vehicleWeight}T) - IMPASSABLE"
                                                        : $"💧 WETLAND: {tags.GetValueOrDefault("name", "Marshy area")} - DIFFICULT, reduce speed"
                                                });
                                            }
                                            
                                            // Check for desert/sand - DIFFICULT for heavy vehicles
                                            if (tags.ContainsKey("natural") && 
                                                (tags["natural"] == "sand" || tags["natural"] == "desert"))
                                            {
                                                obstacleCategories["desert"]++;
                                                var isDifficult = vehicleWeight > 25;
                                                obstacles.Add(new { 
                                                    type = "desert",
                                                    isImpassable = false,
                                                    description = isDifficult
                                                        ? $"🏜️ DESERT/SAND: Heavy vehicle ({vehicleWeight}T) may get stuck - DIFFICULT"
                                                        : $"🏜️ DESERT/SAND: {tags.GetValueOrDefault("name", "Sandy area")} - PASSABLE with caution"
                                                });
                                            }
                                            
                                            // Check for forest - DIFFICULT for wide vehicles
                                            if (tags.ContainsKey("natural") && 
                                                (tags["natural"] == "wood" || tags["natural"] == "forest") ||
                                                tags.ContainsKey("landuse") && tags["landuse"] == "forest")
                                            {
                                                obstacleCategories["forest"]++;
                                                obstacles.Add(new { 
                                                    type = "forest",
                                                    isImpassable = false,
                                                    description = $"🌲 FOREST: {tags.GetValueOrDefault("name", "Forested area")} - PASSABLE, watch for trees"
                                                });
                                            }
                                            
                                            // Check for urban areas - MANEUVERABLE but civilians present
                                            if (tags.ContainsKey("place") && tags["place"] == "city" ||
                                                tags.ContainsKey("landuse") && tags["landuse"] == "residential")
                                            {
                                                obstacleCategories["urban"]++;
                                                obstacles.Add(new { 
                                                    type = "urban",
                                                    isImpassable = false,
                                                    description = $"🏘️ URBAN: {tags.GetValueOrDefault("name", "City/Residential")} - Civilians present, maneuverable"
                                                });
                                            }
                                            
                                            // Check for military zones - RESTRICTED
                                            if (tags.ContainsKey("military") ||
                                                tags.ContainsKey("landuse") && tags["landuse"] == "military")
                                            {
                                                obstacleCategories["military"]++;
                                                obstacles.Add(new { 
                                                    type = "military_zone",
                                                    isImpassable = false,
                                                    description = $"⚠️ MILITARY ZONE: {tags.GetValueOrDefault("name", "Restricted Area")} - RESTRICTED ACCESS"
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get terrain features, using basic analysis");
                    }
                }

                // Fallback to basic analysis if no real data available
                if (elevations.Count == 0)
                {
                    _logger.LogWarning("No elevation data received, using basic fallback analysis");
                    startElevation = double.Parse(markers[0].latitude) * 100; // Basic fallback
                    endElevation = double.Parse(markers[markers.Count - 1].latitude) * 100;
                    
                    // Basic slope calculation
                    for (int i = 0; i < markers.Count - 1; i++)
                    {
                        var elevation1 = double.Parse(markers[i].latitude) * 100;
                        var elevation2 = double.Parse(markers[i + 1].latitude) * 100;
                        var distance = CalculateDistance(
                            double.Parse(markers[i].latitude), double.Parse(markers[i].longitude),
                            double.Parse(markers[i + 1].latitude), double.Parse(markers[i + 1].longitude)
                        );
                        
                        if (distance > 0)
                        {
                            var slope = Math.Atan(Math.Abs(elevation2 - elevation1) / (distance * 1000)) * 180 / Math.PI;
                            maxSlope = Math.Max(maxSlope, slope);
                        }
                    }
                }

                // Determine if route is BLOCKED by impassable obstacles (like TacticalViewer)
                var waterBlocked = obstacleCategories["water"] > 0;
                var cliffBlocked = obstacleCategories["cliff"] > 0;
                var wetlandBlocked = obstacleCategories["wetland"] > 0 && vehicleWeight > 30;
                var steepBlocked = obstacleCategories["steep"] > 0;
                
                var isRouteBlocked = waterBlocked || cliffBlocked || wetlandBlocked || steepBlocked;
                var blockageReason = "";
                
                if (waterBlocked)
                    blockageReason = $"Route BLOCKED by {obstacleCategories["water"]} water crossing(s) - requires bridge/boat";
                else if (cliffBlocked)
                    blockageReason = $"Route BLOCKED by {obstacleCategories["cliff"]} cliff(s)/rock face(s) - impassable terrain";
                else if (wetlandBlocked)
                    blockageReason = $"Route BLOCKED by wetland - vehicle too heavy ({vehicleWeight}T)";
                else if (steepBlocked)
                    blockageReason = $"Route BLOCKED by {obstacleCategories["steep"]} steep slope(s) - exceeds vehicle capability";

                return new
                {
                    startElevation = Math.Round(startElevation, 1),
                    endElevation = Math.Round(endElevation, 1),
                    elevationGain = Math.Round(endElevation - startElevation, 1),
                    maxSlope = Math.Round(maxSlope, 1),
                    obstacles = obstacles.Distinct().ToList(),
                    obstacleCategories = obstacleCategories,
                    terrainType = maxSlope > 15 ? "Mountainous" : maxSlope > 5 ? "Hilly" : "Flat",
                    difficulty = maxSlope > 20 ? "High" : maxSlope > 10 ? "Moderate" : "Low",
                    dataSource = elevations.Count > 0 ? "Real elevation data" : "Basic fallback",
                    elevationPoints = elevations.Count,
                    vehicleWeight = vehicleWeight,
                    // Movement possibility analysis (like TacticalViewer)
                    isRouteBlocked = isRouteBlocked,
                    blockageReason = blockageReason,
                    movementPossible = !isRouteBlocked
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in terrain analysis");
                return new
                {
                    startElevation = 0.0,
                    endElevation = 0.0,
                    elevationGain = 0.0,
                    maxSlope = 0.0,
                    obstacles = new List<object>(),
                    obstacleCategories = new Dictionary<string, int>(),
                    terrainType = "Unknown",
                    difficulty = "Unknown",
                    dataSource = "Error",
                    elevationPoints = 0,
                    vehicleWeight = 0.0,
                    isRouteBlocked = false,
                    blockageReason = "",
                    movementPossible = true,
                    error = ex.Message
                };
            }
        }

        private async Task<dynamic> CallElevationLookup(object request, string terrainDb)
        {
            try
            {
                _logger.LogInformation($"Calling elevation lookup with terrainDb: {terrainDb}");
                
                if (string.IsNullOrEmpty(terrainDb))
                {
                    _logger.LogWarning("Terrain database path is empty");
                    return new { success = false, error = "Terrain database path is empty" };
                }

                var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                // Convert web path (forward slashes) to OS path
                var normalizedTerrainDb = terrainDb.Replace('/', Path.DirectorySeparatorChar);
                var dbPath = Path.Combine(wwwRoot, normalizedTerrainDb);

                if (!System.IO.File.Exists(dbPath))
                {
                    _logger.LogWarning($"Terrain database not found at: {dbPath}");
                    return new { success = false, error = "Terrain database not found" };
                }

                using var context = new Services.MapManagement.TerrainDataContext(dbPath);
                await context.InitializeDatabaseAsync();

                var datasetId = await GetTerrainDatasetIdAsync(context);
                if (datasetId == Guid.Empty)
                {
                    _logger.LogWarning("No terrain dataset found in database");
                    return new { success = false, error = "No terrain dataset found in database" };
                }

                var results = new List<object>();
                var locations = ((dynamic)request).locations;
                
                foreach (var loc in locations)
                {
                    var lat = (double)loc.latitude;
                    var lng = (double)loc.longitude;
                    
                    var elevationPoints = await context.QueryElevationPointsAsync(
                        lat - 0.01,
                        lat + 0.01,
                        lng - 0.01,
                        lng + 0.01,
                        datasetId
                    );
                    
                    double elevation = 0;
                    if (elevationPoints.Any())
                    {
                        var closest = elevationPoints
                            .OrderBy(p => Math.Pow(p.Latitude - lat, 2) + Math.Pow(p.Longitude - lng, 2))
                            .First();
                        elevation = closest.ElevationMeters;
                    }
                    results.Add(new { latitude = lat, longitude = lng, elevation });
                }

                _logger.LogInformation($"Elevation lookup successful, returning {results.Count} results");
                return new { success = true, results = results };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling elevation lookup API");
                return new { success = false, error = ex.Message };
            }
        }

        private async Task<Guid> GetTerrainDatasetIdAsync(Services.MapManagement.TerrainDataContext context)
        {
            var conn = context.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id FROM terrain_datasets LIMIT 1";
            var result = await cmd.ExecuteScalarAsync();
            if (result != null && Guid.TryParse(result.ToString(), out var id)) return id;
            return Guid.Empty;
        }

        private async Task<dynamic> CallTerrainFeatures(double[] bbox, string terrainDb)
        {
            try
            {
                _logger.LogInformation($"Calling terrain features with terrainDb: {terrainDb}");
                
                if (string.IsNullOrEmpty(terrainDb))
                {
                    _logger.LogWarning("Terrain database path is empty for terrain features");
                    return new { success = false, error = "Terrain database path is empty" };
                }

                var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var normalizedTerrainDb = terrainDb.Replace('/', Path.DirectorySeparatorChar);
                var dbPath = Path.Combine(wwwRoot, normalizedTerrainDb);

                if (!System.IO.File.Exists(dbPath))
                {
                    _logger.LogWarning($"Terrain database not found at: {dbPath}");
                    return new { success = false, error = "Terrain database not found" };
                }

                using var context = new Services.MapManagement.TerrainDataContext(dbPath);
                await context.InitializeDatabaseAsync();
                
                var datasetId = await GetTerrainDatasetIdAsync(context);
                if (datasetId == Guid.Empty)
                {
                    _logger.LogWarning("No terrain dataset found in database for terrain features");
                    return new { success = false, error = "No terrain dataset found in database" };
                }

                var features = await context.QueryTerrainFeaturesAsync(
                    bbox[0], bbox[2], bbox[1], bbox[3], datasetId, null
                );

                var elements = features.Select(f => new
                {
                    id = f.OsmId,
                    type = "way",
                    tags = string.IsNullOrEmpty(f.TagsJson)
                        ? new Dictionary<string, string>()
                        : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(f.TagsJson),
                    geometry = ParseGeometryToOverpassFormat(f.GeometryJson)
                }).ToList();

                _logger.LogInformation($"Terrain features lookup successful, returning {elements.Count} features");
                return new { success = true, elements = elements };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling terrain features API");
                return new { success = false, error = ex.Message };
            }
        }

        private object ParseGeometryToOverpassFormat(string geometryJson)
        {
            try
            {
                if (string.IsNullOrEmpty(geometryJson))
                    return new { type = "LineString", coordinates = new double[0][] };

                var geometry = System.Text.Json.JsonSerializer.Deserialize<dynamic>(geometryJson);
                return geometry;
            }
            catch
            {
                return new { type = "LineString", coordinates = new double[0][] };
            }
        }

        private object CalculateRealisticTimeEstimate(double totalDistance, string mobilityType, 
            MilitaryUnit militaryUnit, object terrainAnalysis)
        {
            try
            {
                // Get base speed based on unit type and mobility
                double baseSpeed = GetRealisticBaseSpeed(mobilityType, militaryUnit);
                
                // Apply terrain modifiers
                double terrainModifier = 1.0;
                double maxSlope = 0.0;
                var obstacles = new List<object>();
                
                try
                {
                    var terrain = terrainAnalysis as dynamic;
                    if (terrain != null)
                    {
                        maxSlope = (double)(terrain.maxSlope ?? 0.0);
                        obstacles = terrain.obstacles as List<object> ?? new List<object>();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error accessing terrain analysis data");
                }
                
                if (maxSlope > 20)
                    terrainModifier = 0.3; // Very steep terrain
                else if (maxSlope > 15)
                    terrainModifier = 0.5; // Steep terrain
                else if (maxSlope > 10)
                    terrainModifier = 0.7; // Moderate terrain
                else if (maxSlope > 5)
                    terrainModifier = 0.85; // Slight terrain

                // Apply obstacle penalties
                foreach (var obstacle in obstacles)
                {
                    try
                    {
                        var obs = obstacle as dynamic;
                        if (obs != null)
                        {
                            string obsType = obs.type?.ToString() ?? "";
                            if (obsType == "water")
                                terrainModifier *= 0.1; // Major delay for water crossing
                            else if (obsType == "steep_terrain")
                                terrainModifier *= 0.6; // Significant delay for steep terrain
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing obstacle data");
                    }
                }

                var effectiveSpeed = baseSpeed * terrainModifier;
                var totalHours = totalDistance / effectiveSpeed;

                return new
                {
                    hours = (int)totalHours,
                    minutes = (int)((totalHours - (int)totalHours) * 60),
                    totalHours = totalHours,
                    baseSpeed = baseSpeed,
                    effectiveSpeed = Math.Round(effectiveSpeed, 1),
                    terrainModifier = Math.Round(terrainModifier, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating realistic time estimate");
                return new
                {
                    hours = 0,
                    minutes = 0,
                    totalHours = 0.0,
                    baseSpeed = 0.0,
                    effectiveSpeed = 0.0,
                    terrainModifier = 1.0,
                    error = ex.Message
                };
            }
        }

        private double GetRealisticBaseSpeed(string mobilityType, MilitaryUnit militaryUnit)
        {
            if (militaryUnit == null)
                return GetBaseSpeed(mobilityType);

            return militaryUnit switch
            {
                InfantryBattalion infantry => (double)infantry.MarchingSpeedCrossCountry,
                ArmouredRegiment armour => (double)armour.MarchingSpeedCrossCountry,
                ArtilleryRegiment artillery => 15.0, // Artillery typically slower
                LogisticsUnit logistics => (double)logistics.MarchingSpeedCrossCountry,
                CombatEngineeringCompany engineers => (double)engineers.MarchingSpeedCrossCountry,
                _ => GetBaseSpeed(mobilityType)
            };
        }

        /// <summary>
        /// Get all placed tokens with their positions based on user's TeamId
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPlacedTokens()
        {
            try
            {
                var placedTokensvar = _context.Tokens
                    .Where(t => t.MapMarkers.Any(m => m.IsActive))
                    .Include(t => t.MapMarkers.OrderByDescending(x => x.CreatedDate))
                    .Include(t => t.TokenGroup)
                    .Include(t => t.AreaCoverages)
                    .Select(t => new
                    {
                        id = t.Id,
                        name = t.Name,
                        tokenGroupId = t.TokenGroupId,
                        tokenGroupName = t.TokenGroup != null ? t.TokenGroup.Name : null,
                        assetImagePath = t.AssetImagePath,
                        position = t.MapMarkers.Where(m => m.IsActive).OrderByDescending(m => m.CreatedDate).Select(m => new { lat = m.latitude, lng = m.longitude }).FirstOrDefault(),
                        isActive = t.IsActive,
                        isManualToken = t.IsManualToken,
                        lastUsed = t.LastUsed,
                        usageCount = t.UsageCount,
                        notes = t.Notes,
                        t.TeamId,
                        status = "placed",
                        forceType = t.ForceType,
                        // Military Unit Classification
                        organizationLevel = t.OrganizationLevel,
                        unitType = t.UnitType,
                        unitDesignation = t.UnitDesignation,
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

                var placedTokens = await placedTokensvar.ToListAsync();
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

                // UnitDeployment removed - using Brigade system now
                // Movement orders no longer require UnitDeployment
                // Skip movement order creation - deprecated system
                _logger.LogInformation("Movement order skipped for token {TokenId} - UnitDeployment system removed",
                    request.TokenId);
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
                        position = t.MapMarkers.Where(m => m.IsActive).OrderByDescending(m => m.CreatedDate).Select(m => new { lat = m.latitude, lng = m.longitude }).FirstOrDefault(),
                        status = "Deployed",
                        teamId = t.TeamId,
                        isActive = t.IsActive
                    })
                    .ToListAsync();

                return Json(new
                {
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

                return Json(new
                {
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
                            currentPosition = t.MapMarkers.Where(m => m.IsActive).OrderByDescending(m => m.CreatedDate).Select(m => new { lat = decimal.Parse(m.latitude), lng = decimal.Parse(m.longitude) }).FirstOrDefault()
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

                return Json(new
                {
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
                            return Json(new
                            {
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
                    .Where(t => t.IsActive && t.TeamId == team.Id)
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

    public class SetTerrainDatabaseRequest
    {
        public string TerrainDbPath { get; set; }
    }
}
