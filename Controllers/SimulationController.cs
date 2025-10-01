using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;
using CombatResult = TechWebSol.Models.CombatResult;

namespace TechWebSol.Controllers
{
    [AuthorizeDynamic]
    public class SimulationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserSessionService _userSessionService;
        private readonly MovementService _movementService;
        private readonly CombatService _combatService;
        private readonly SupplyService _supplyService;
        private readonly IOrderPersistenceService _orderPersistenceService;
        private readonly IDetectionService _detectionService;

        public SimulationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IUserSessionService userSessionService,
            MovementService movementService,
            CombatService combatService,
            SupplyService supplyService,
            IOrderPersistenceService orderPersistenceService,
            IDetectionService detectionService)
        {
            _context = context;
            _userManager = userManager;
            _userSessionService = userSessionService;
            _movementService = movementService;
            _combatService = combatService;
            _supplyService = supplyService;
            _orderPersistenceService = orderPersistenceService;
            _detectionService = detectionService;
        }

        #region Scenario Management

        [HttpGet]
        public async Task<IActionResult> GetScenarios()
        {
            var user = await _userManager.GetUserAsync(User);
            var scenarios = await _context.WarGameScenarios
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            return Json(scenarios);
        }

        [HttpPost]
        public async Task<IActionResult> CreateScenario([FromBody] WarGameScenario scenario)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                scenario.Id = Guid.NewGuid();
                scenario.CreatedBy = user.FullName;
                scenario.IsActive = true;
                scenario.Status = "Planning";

                _context.WarGameScenarios.Add(scenario);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = scenario });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> StartScenario([FromBody] StartScenarioRequest request)
        {
            try
            {
                var scenario = await _context.WarGameScenarios
                    .FirstOrDefaultAsync(s => s.Id == Guid.Parse(request.ScenarioId));

                if (scenario == null) return NotFound();

                scenario.Status = "Active";

                // Create simulation event
                var startEvent = new SimulationEvent
                {
                    Id = Guid.NewGuid(),
                    ScenarioId = scenario.Id,
                    EventType = "ScenarioStart",
                    EventData = JsonSerializer.Serialize(new { ScenarioName = scenario.Name }),
                    EventTime = DateTime.UtcNow,
                    TriggeredByUserId = (await _userManager.GetUserAsync(User))?.Id,
                    TriggeredByUserName = (await _userManager.GetUserAsync(User))?.FullName,
                    IsActive = true
                };

                _context.SimulationEvents.Add(startEvent);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Unit Deployment

        [HttpPost]
        public async Task<IActionResult> DeployUnit([FromBody] DeployUnitRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                // Get unit data based on type
                object unitData = null;
                switch (request.UnitType.ToLower())
                {
                    case "infantry":
                        unitData = await _context.InfantryBattalions
                            .FirstOrDefaultAsync(u => u.Id ==request.UnitId && u.TeamId == user.TeamId);
                        break;
                    case "armoured":
                        unitData = await _context.ArmouredRegiments
                            .FirstOrDefaultAsync(u => u.Id == request.UnitId && u.TeamId == user.TeamId);
                        break;
                    case "artillery":
                        unitData = await _context.ArtilleryRegiments
                            .FirstOrDefaultAsync(u => u.Id == request.UnitId && u.TeamId == user.TeamId);
                        break;
                }

                if (unitData == null) return NotFound("Unit not found");

                var deployment = new UnitDeployment
                {
                    Id = Guid.NewGuid(),
                    ScenarioId = request.ScenarioId,
                    UnitId = request.UnitId,
                    UnitType = request.UnitType,
                    UnitName = GetUnitName(unitData),
                    ForceType = GetUnitForceType(unitData),
                    Position = JsonSerializer.Serialize(request.Position),
                    Formation = request.Formation ?? "Line",
                    Status = "Deployed",
                    CurrentStrength = GetUnitStrength(unitData),
                    MaxStrength = GetUnitStrength(unitData),
                    Morale = 100,
                    Fatigue = 0,
                    TeamId = user.TeamId,
                    IsActive = true
                };

                _context.UnitDeployments.Add(deployment);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = deployment });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDeployedUnits(Guid? scenarioId)
        {
            var deployments = await _context.UnitDeployments
                .Where(d => d.ScenarioId == scenarioId && d.IsActive)
                .ToListAsync();

            return Json(deployments);
        }

        #endregion

        #region Movement Orders

        [HttpPost]
        public async Task<IActionResult> IssueMovementOrder([FromBody] MovementOrderRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var deployment = await _context.UnitDeployments
                    .FirstOrDefaultAsync(d => d.Id == request.UnitDeploymentId && d.TeamId == user.TeamId);

                if (deployment == null) return NotFound("Unit deployment not found");

                // Calculate movement details
                var movementCalc = CalculateMovement(deployment, request);

                var movementOrder = new MovementOrder
                {
                    Id = Guid.NewGuid(),
                    UnitDeploymentId = request.UnitDeploymentId,
                    StartPosition = deployment.Position,
                    EndPosition = JsonSerializer.Serialize(request.EndPosition),
                    Waypoints = request.Waypoints != null ? JsonSerializer.Serialize(request.Waypoints) : null,
                    MovementType = request.MovementType,
                    Status = "Planned",
                    Speed = movementCalc.EffectiveSpeed,
                    Distance = (decimal)movementCalc.Distance,
                    EstimatedArrival = DateTime.UtcNow.AddHours((double)movementCalc.TravelTime),
                    TerrainFactors = JsonSerializer.Serialize(movementCalc.TerrainTypes),
                    CreatedBy = user.FullName,
                    IsActive = true
                };

                _context.MovementOrders.Add(movementOrder);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = movementOrder, calculation = movementCalc });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteMovementOrder([FromBody] ExecuteMovementRequest request)
        {
            try
            {
                var movementOrder = await _context.MovementOrders
                    .Include(m => m.UnitDeployment)
                    .FirstOrDefaultAsync(m => m.Id == Guid.Parse(request.MovementOrderId));

                if (movementOrder == null) return NotFound();

                movementOrder.Status = "InProgress";
                movementOrder.StartTime = DateTime.UtcNow;

                // Update unit deployment position and status
                movementOrder.UnitDeployment.Status = "Moving";
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Combat System

        [HttpPost]
        public async Task<IActionResult> InitiateBattle([FromBody] InitiateBattleRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var battle = new Battle
                {
                    Id = Guid.NewGuid(),
                    BattleName = request.BattleName,
                    ScenarioId = request.ScenarioId,
                    BattleLocation = JsonSerializer.Serialize(request.BattleLocation),
                    BattleType = request.BattleType,
                    Status = "Planned",
                    TerrainType = request.TerrainType,
                    TerrainModifier = GetTerrainModifier(request.TerrainType),
                    WeatherConditions = request.WeatherConditions ?? "Clear",
                    WeatherModifier = GetWeatherModifier(request.WeatherConditions ?? "Clear"),
                    IsActive = true
                };

                _context.Battles.Add(battle);

                // Add participating units
                foreach (var unitId in request.ParticipatingUnits)
                {
                    var deployment = await _context.UnitDeployments
                        .FirstOrDefaultAsync(d => d.Id == Guid.Parse(unitId));

                    if (deployment != null)
                    {
                        var participant = new BattleParticipant
                        {
                            Id = Guid.NewGuid(),
                            BattleId = battle.Id,
                            UnitDeploymentId = deployment.Id,
                            Role = DetermineRole(deployment, request.BattleType),
                            InitialStrength = deployment.CurrentStrength,
                            FinalStrength = deployment.CurrentStrength,
                            CombatEffectiveness = CalculateCombatEffectiveness(deployment),
                            ProtectionFactor = await GetProtectionFactor(deployment),
                            Position = deployment.Position,
                            IsActive = true
                        };

                        _context.BattleParticipants.Add(participant);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, data = battle });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResolveBattle([FromBody] ResolveBattleRequest request)
        {
            try
            {
                var battle = await _context.Battles
                    .Include(b => b.Participants)
                    .ThenInclude(p => p.UnitDeployment)
                    .FirstOrDefaultAsync(b => b.Id == Guid.Parse(request.BattleId));

                if (battle == null) return NotFound();

                battle.Status = "Active";
                battle.StartTime = DateTime.UtcNow;

                var blueForces = battle.Participants.Where(p => p.UnitDeployment.ForceType == "Blue").ToList();
                var redForces = battle.Participants.Where(p => p.UnitDeployment.ForceType == "Red").ToList();

                var combatResult = CalculateCombatResult(blueForces, redForces, battle);

                // Apply casualties
                ApplyCasualties(blueForces, (decimal)combatResult.AttackerCasualties);
                ApplyCasualties(redForces, (decimal)combatResult.DefenderCasualties);

                // Determine victor
                battle.Victor = combatResult.Victor;
                battle.EndTime = DateTime.UtcNow;
                battle.Status = "Resolved";
                battle.BattleResults = JsonSerializer.Serialize(combatResult);

                // Create combat result record
                var combatResultRecord = new CombatResult
                {
                    Id = Guid.NewGuid(),
                    BattleId = battle.Id,
                    AttackerId = blueForces.FirstOrDefault()?.Id,
                    DefenderId = redForces.FirstOrDefault()?.Id,
                    CombatType = battle.BattleType,
                    AttackerStrength = (decimal)combatResult.AttackerFirepower,
                    DefenderStrength = (decimal)combatResult.DefenderFirepower,
                    AttackerLosses = (decimal)combatResult.AttackerCasualties,
                    DefenderLosses = (decimal)combatResult.DefenderCasualties,
                    TerrainModifier = (decimal)combatResult.TerrainModifier,
                    ProtectionModifier = (decimal)combatResult.AttackerProtection,
                    Result = combatResult.Victor,
                    CombatTime = DateTime.UtcNow,
                    CombatDetails = JsonSerializer.Serialize(combatResult),
                    IsActive = true
                };

                _context.CombatResults.Add(combatResultRecord);
                await _context.SaveChangesAsync();

                return Json(new { success = true, result = combatResult });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Objectives

        [HttpPost]
        public async Task<IActionResult> CreateObjective([FromBody] Objective objective)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                objective.Id = Guid.NewGuid();
                objective.Status = "Pending";
                objective.CreatedBy = user.FullName;
                objective.IsActive = true;

                _context.Objectives.Add(objective);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = objective });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetObjectives(Guid? scenarioId)
        {
            var objectives = await _context.Objectives
                .Where(o => o.ScenarioId == scenarioId && o.IsActive)
                .ToListAsync();

            return Json(objectives);
        }

        #endregion

        #region Calculation Methods

        private MovementCalculation CalculateMovement(UnitDeployment deployment, MovementOrderRequest request)
        {
            // Get unit's base speed based on type and movement type
            var baseSpeed = GetUnitSpeed(deployment, request.MovementType);
            
            // Apply terrain modifiers
            var terrainModifier = GetTerrainModifier(request.TerrainType ?? "Plain");
            
            // Apply fatigue modifier
            var fatigueModifier = 1.0m - (deployment.Fatigue / 100m * 0.5m);
            
            // Calculate effective speed
            var effectiveSpeed = baseSpeed * terrainModifier * fatigueModifier;
            
            // Calculate distance (simplified - in real implementation would use proper geo calculations)
            var distance = CalculateDistance(
                JsonSerializer.Deserialize<Position>(deployment.Position),
                request.EndPosition
            );
            
            // Calculate travel time
            var travelTime = distance / (double)effectiveSpeed;

            return new MovementCalculation
            {
                BaseSpeed = (decimal)baseSpeed,
                TerrainModifier = (decimal)terrainModifier,
                FatigueModifier = (decimal)fatigueModifier,
                EffectiveSpeed = effectiveSpeed,
                Distance = (decimal)distance,
                TravelTime = (decimal)travelTime,
                TerrainTypes = request.TerrainType ?? "Plain"
            };
        }

        private CombatCalculation CalculateCombatResult(
            List<BattleParticipant> attackers,
            List<BattleParticipant> defenders,
            Battle battle)
        {
            // Calculate total firepower for each side
            var attackerFirepower = attackers.Sum(a => CalculateFirepower(a));
            var defenderFirepower = defenders.Sum(d => CalculateFirepower(d));

            // Calculate protection factors
            var attackerProtection = attackers.Average(a => (double)a.ProtectionFactor);
            var defenderProtection = defenders.Average(d => (double)d.ProtectionFactor);

            // Apply modifiers
            var terrainMod = (double)battle.TerrainModifier;
            var weatherMod = (double)battle.WeatherModifier;
            var moraleMod = attackers.Average(a => (double)a.UnitDeployment.Morale) / 100.0;
            var fatigueMod = 1.0 - (attackers.Average(a => (double)a.UnitDeployment.Fatigue) / 100.0 * 0.3);

            // Calculate casualties using simplified Lanchester equations
            var attackerEffectiveness = attackerFirepower * terrainMod * weatherMod * moraleMod * fatigueMod;
            var defenderEffectiveness = defenderFirepower * terrainMod * weatherMod * defenderProtection;

            // Calculate casualties as percentage of strength
            var attackerCasualties = Math.Min(50, defenderEffectiveness / attackerEffectiveness * 20);
            var defenderCasualties = Math.Min(50, attackerEffectiveness / defenderEffectiveness * 25); // Attackers have slight advantage

            // Determine victor
            string victor = "Draw";
            var victoryMargin = 0.0;
            
            if (attackerEffectiveness > defenderEffectiveness * 1.2)
            {
                victor = "Blue";
                victoryMargin = (attackerEffectiveness - defenderEffectiveness) / defenderEffectiveness;
            }
            else if (defenderEffectiveness > attackerEffectiveness * 1.1)
            {
                victor = "Red";
                victoryMargin = (defenderEffectiveness - attackerEffectiveness) / attackerEffectiveness;
            }

            return new CombatCalculation
            {
                AttackerFirepower = (decimal)attackerFirepower,
                DefenderFirepower = (decimal)defenderFirepower,
                AttackerProtection = (decimal)attackerProtection,
                DefenderProtection = (decimal)defenderProtection,
                TerrainModifier = (decimal)terrainMod,
                WeatherModifier = (decimal)weatherMod,
                MoraleModifier = (decimal)moraleMod,
                FatigueModifier = (decimal)fatigueMod,
                AttackerCasualties = (decimal)attackerCasualties,
                DefenderCasualties = (decimal)defenderCasualties,
                Victor = victor,
                VictoryMargin = (decimal)victoryMargin
            };
        }

        private double CalculateFirepower(BattleParticipant participant)
        {
            // Get unit data and calculate firepower based on equipment
            var baseFirepower = participant.InitialStrength * 1.0; // Base firepower per soldier
            
            // Apply combat effectiveness modifier
            return baseFirepower * (double)participant.CombatEffectiveness;
        }

        private decimal CalculateCombatEffectiveness(UnitDeployment deployment)
        {
            // Base effectiveness modified by morale and fatigue
            var baseEffectiveness = 1.0m;
            var moraleModifier = deployment.Morale / 100m;
            var fatigueModifier = 1.0m - (deployment.Fatigue / 100m * 0.3m);
            
            return baseEffectiveness * moraleModifier * fatigueModifier;
        }

        private async Task<decimal> GetProtectionFactor(UnitDeployment deployment)
        {
            // Get protection factor from ForceProtection table
            var protection = await _context.ForceProtections
                .FirstOrDefaultAsync(p => p.ForceType == deployment.ForceType && p.TeamId == deployment.TeamId);
            
            return protection?.ProtectionFactor ?? 1.0m;
        }

        private decimal GetTerrainModifier(string terrainType)
        {
            // Simplified terrain modifiers - in reality would come from TerrainMobilityFactor table
            return terrainType?.ToLower() switch
            {
                "plain" => 1.0m,
                "forest" => 0.7m,
                "mountain" => 0.5m,
                "desert" => 0.8m,
                "swamp" => 0.3m,
                "urban" => 0.6m,
                _ => 1.0m
            };
        }

        private decimal GetWeatherModifier(string weather)
        {
            return weather?.ToLower() switch
            {
                "clear" => 1.0m,
                "rain" => 0.8m,
                "snow" => 0.6m,
                "fog" => 0.7m,
                "storm" => 0.5m,
                _ => 1.0m
            };
        }

        private decimal GetUnitSpeed(UnitDeployment deployment, string movementType)
        {
            // Would fetch from unit data - simplified for now
            return movementType?.ToLower() switch
            {
                "march" => deployment.UnitType.ToLower() switch
                {
                    "infantry" => 30m, // km/h on roads
                    "armoured" => 15m,
                    "artillery" => 12m,
                    _ => 20m
                },
                "tactical" => 10m, // Slower for tactical movement
                "combat" => 1m, // Very slow in combat
                _ => 20m
            };
        }

        private double CalculateDistance(Position start, Position end)
        {
            // Simplified distance calculation - in reality would use proper geodesic calculations
            var latDiff = end.Lat - start.Lat;
            var lngDiff = end.Lng - start.Lng;
            return Math.Sqrt(latDiff * latDiff + lngDiff * lngDiff) * 111; // Rough km conversion
        }

        private void ApplyCasualties(List<BattleParticipant> participants, decimal casualtyPercentage)
        {
            foreach (var participant in participants)
            {
                var casualties = (int)(participant.InitialStrength * casualtyPercentage / 100);
                participant.Casualties = casualties;
                participant.FinalStrength = Math.Max(0, participant.InitialStrength - casualties);
                participant.UnitDeployment.CurrentStrength = participant.FinalStrength;
                // UpdatedBy handled automatically by BaseEntity
            }
        }

        private string DetermineRole(UnitDeployment deployment, string battleType)
        {
            // Simplified role determination
            return battleType.ToLower() switch
            {
                "assault" => deployment.ForceType == "Blue" ? "Attacker" : "Defender",
                "defense" => deployment.ForceType == "Blue" ? "Defender" : "Attacker",
                _ => "Support"
            };
        }

        // Helper methods for unit data extraction
        private string GetUnitName(object unitData)
        {
            return unitData switch
            {
                InfantryBattalion infantry => infantry.Name,
                ArmouredRegiment armoured => armoured.Name,
                ArtilleryRegiment artillery => artillery.Name,
                _ => "Unknown Unit"
            };
        }

        private string GetUnitForceType(object unitData)
        {
            return unitData switch
            {
                InfantryBattalion infantry => infantry.ForceType,
                ArmouredRegiment armoured => armoured.ForceType,
                ArtilleryRegiment artillery => artillery.ForceType,
                _ => "Unknown"
            };
        }

        private int GetUnitStrength(object unitData)
        {
            return unitData switch
            {
                InfantryBattalion infantry => infantry.Strength,
                ArmouredRegiment armoured => armoured.Strength,
                ArtilleryRegiment artillery => artillery.Strength,
                _ => 0
            };
        }

        #endregion

        #region Enhanced Simulation Methods

        [HttpPost]
        public async Task<IActionResult> AdvanceTurn([FromBody] AdvanceTurnRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var scenario = await _context.WarGameScenarios
                    .Include(s => s.UnitDeployments)
                    .ThenInclude(ud => ud.MovementOrders)
                    .FirstOrDefaultAsync(s => s.Id == Guid.Parse(request.ScenarioId));

                if (scenario == null) return NotFound();

                // Process movement orders
                var activeDeployments = scenario.UnitDeployments.Where(ud => ud.IsActive).ToList();
                
                foreach (var deployment in activeDeployments)
                {
                    var activeMovementOrder = deployment.MovementOrders.FirstOrDefault(m => m.Status == "InProgress");
                    if (activeMovementOrder != null)
                    {
                        var terrainType = await _movementService.GetTerrainAtPosition(0, 0); // Simplified for now
                        var movementCost = await _movementService.CalculateMovementCost(terrainType, "road");

                        deployment.RemainingMovement -= movementCost;
                        if (deployment.RemainingMovement <= 0)
                        {
                            activeMovementOrder.Status = "Completed";
                            activeMovementOrder.ActualArrival = DateTime.UtcNow;
                            deployment.RemainingMovement = deployment.MovementPoints;
                        }
                    }
                    else
                    {
                        // Reset movement points for units not moving
                        deployment.RemainingMovement = deployment.MovementPoints;
                    }
                }

                // Process supply degradation
                foreach (var deployment in activeDeployments)
                {
                    _supplyService.DegradeSupply(deployment, request.SupplyDegradationRate);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Turn advanced successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSupplyState([FromBody] UpdateSupplyRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var deployment = await _context.UnitDeployments
                    .FirstOrDefaultAsync(d => d.Id == Guid.Parse(request.UnitDeploymentId) && d.TeamId == user.TeamId);

                if (deployment == null) return NotFound();

                _supplyService.UpdateSupplyState(deployment, request.SupplyState);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Supply state updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CalculateMovement([FromBody] CalculateMovementRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var deployment = await _context.UnitDeployments
                    .FirstOrDefaultAsync(d => d.Id == Guid.Parse(request.UnitDeploymentId) && d.TeamId == user.TeamId);

                if (deployment == null) return NotFound();

                var effectiveMovement = await _movementService.GetEffectiveMovement(deployment);
                var canMove = await _movementService.CanMove(deployment, request.Distance);

                return Json(new { 
                    success = true, 
                    effectiveMovement = effectiveMovement,
                    canMove = canMove,
                    requiredMovement = await _movementService.CalculateMovementCost(request.TerrainType, "road")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Attack Order Execution

        /// <summary>
        /// Execute a specific attack order
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ExecuteAttackOrder([FromBody] ExecuteAttackOrderRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var order = await _orderPersistenceService.GetAttackOrderAsync(request.OrderId);
                if (order == null)
                {
                    return NotFound(new { success = false, message = "Attack order not found" });
                }

                // Verify user has access to this order
                if (order.TeamId != user.TeamId)
                {
                    return Forbid("Access denied to this attack order");
                }

                // Check if order can be executed
                if (order.Status != "Planned" && order.Status != "Executing")
                {
                    return BadRequest(new { success = false, message = $"Cannot execute order with status: {order.Status}" });
                }

                // Update status to executing
                await _orderPersistenceService.UpdateStatusAsync(order.Id, "Executing");

                // Execute the attack
                var result = await ExecuteAttackOrderInternal(order);

                // Update final status
                await _orderPersistenceService.UpdateStatusAsync(order.Id, result.Success ? "Completed" : "Failed");

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    casualties = result.Casualties,
                    orderId = order.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error executing attack order" });
            }
        }

        /// <summary>
        /// Execute all due attack orders for the current turn
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AdvanceTurnExecutePlannedAttacks([FromBody] AdvanceTurnRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var dueOrders = await _orderPersistenceService.GetDueOrdersAsync(request.CurrentTurn);
                var results = new List<object>();

                foreach (var order in dueOrders)
                {
                    try
                    {
                        // Only execute orders for this user's team
                        if (order.TeamId != user.TeamId) continue;

                        var result = await ExecuteAttackOrderInternal(order);
                        await _orderPersistenceService.UpdateStatusAsync(order.Id, result.Success ? "Completed" : "Failed");

                        results.Add(new
                        {
                            orderId = order.Id,
                            success = result.Success,
                            message = result.Message,
                            casualties = result.Casualties
                        });
                    }
                    catch (Exception ex)
                    {
                        await _orderPersistenceService.UpdateStatusAsync(order.Id, "Failed");
                        results.Add(new
                        {
                            orderId = order.Id,
                            success = false,
                            message = "Execution failed",
                            error = ex.Message
                        });
                    }
                }

                return Json(new
                {
                    success = true,
                    executedOrders = results.Count,
                    results = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error executing planned attacks" });
            }
        }

        /// <summary>
        /// Internal method to execute an attack order
        /// </summary>
        private async Task<AttackExecutionResult> ExecuteAttackOrderInternal(AttackOrder order)
        {
            try
            {
                // Get attacker and target deployments
                var attackerDeployment = await _context.UnitDeployments
                    .FirstOrDefaultAsync(d => d.TokenId == order.AttackerTokenId);

                var targetDeployment = await _context.UnitDeployments
                    .FirstOrDefaultAsync(d => d.TokenId == order.TargetTokenId);

                if (attackerDeployment == null || targetDeployment == null)
                {
                    return new AttackExecutionResult
                    {
                        Success = false,
                        Message = "Attacker or target deployment not found"
                    };
                }

                // Check detection confidence
                var detectionConfidence = await _detectionService.GetDetectionConfidenceAsync(
                    order.AttackerTokenId.ToString(), order.TargetTokenId.ToString());

                if (detectionConfidence < 0.3)
                {
                    return new AttackExecutionResult
                    {
                        Success = false,
                        Message = "Target cannot be detected - attack aborted"
                    };
                }

                // Calculate movement needed
                var attackerToken = await _context.Tokens
                    .Include(t => t.MapMarkers.Where(m => m.IsActive))
                    .FirstOrDefaultAsync(t => t.Id == order.AttackerTokenId);

                var targetToken = await _context.Tokens
                    .Include(t => t.MapMarkers.Where(m => m.IsActive))
                    .FirstOrDefaultAsync(t => t.Id == order.TargetTokenId);

                if (attackerToken?.MapMarkers?.FirstOrDefault() == null || 
                    targetToken?.MapMarkers?.FirstOrDefault() == null)
                {
                    return new AttackExecutionResult
                    {
                        Success = false,
                        Message = "Attacker or target position not found"
                    };
                }

                // Calculate distance and movement cost
                var attackerLat = double.Parse(attackerToken.MapMarkers.First().latitude);
                var attackerLng = double.Parse(attackerToken.MapMarkers.First().longitude);
                var targetLat = double.Parse(targetToken.MapMarkers.First().latitude);
                var targetLng = double.Parse(targetToken.MapMarkers.First().longitude);

                var distance = CalculateDistance(attackerLat, attackerLng, targetLat, targetLng);
                var movementCost = distance; // Simplified - in reality would use terrain modifiers

                // Check if attacker has enough movement points
                var requiredMovement = movementCost + (attackerDeployment.MovementPointsPerTurn * (double)order.MpReservePercent);
                if (attackerDeployment.RemainingMovementPoints < requiredMovement)
                {
                    return new AttackExecutionResult
                    {
                        Success = false,
                        Message = "Insufficient movement points for attack"
                    };
                }

                // Deduct movement points
                attackerDeployment.RemainingMovementPoints -= (int)requiredMovement;

                // Execute combat
                var combatResult = _combatService.ResolveCombat(
                    ConvertToMilitaryUnit(attackerDeployment),
                    ConvertToMilitaryUnit(targetDeployment),
                    attackerDeployment.CurrentTerrain);

                // Update unit strengths
                attackerDeployment.CurrentStrength = (int)(attackerDeployment.CurrentStrength * (1.0 - combatResult.AttackerCasualties / 100.0));
                targetDeployment.CurrentStrength = (int)(targetDeployment.CurrentStrength * (1.0 - combatResult.DefenderCasualties / 100.0));

                // Update strength percentages
                attackerDeployment.StrengthPercentage = (attackerDeployment.CurrentStrength / (double)attackerDeployment.MaxStrength) * 100.0;
                targetDeployment.StrengthPercentage = (targetDeployment.CurrentStrength / (double)targetDeployment.MaxStrength) * 100.0;

                // Save changes
                await _context.SaveChangesAsync();

                return new AttackExecutionResult
                {
                    Success = true,
                    Message = "Attack executed successfully",
                    Casualties = new
                    {
                        attacker = combatResult.AttackerCasualties,
                        defender = combatResult.DefenderCasualties
                    }
                };
            }
            catch (Exception ex)
            {
                return new AttackExecutionResult
                {
                    Success = false,
                    Message = $"Attack execution failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Convert UnitDeployment to MilitaryUnit for combat calculations
        /// </summary>
        private MilitaryUnit ConvertToMilitaryUnit(UnitDeployment deployment)
        {
            return new MilitaryUnit
            {
                StrengthPercentage = deployment.StrengthPercentage,
                CombatPower = deployment.CombatPowerIndex,
                TerrainModifier = deployment.TerrainModifier,
                SupplyState = deployment.SupplyStateInt,
                CurrentTerrain = deployment.CurrentTerrain
            };
        }

        /// <summary>
        /// Calculate distance between two points in kilometers
        /// </summary>
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

    // Request/Response DTOs
    public class StartScenarioRequest
    {
        public string ScenarioId { get; set; }
    }

    public class DeployUnitRequest
    {
        public Guid? ScenarioId { get; set; }
        public Guid? UnitId { get; set; }
        public string UnitType { get; set; }
        public Position Position { get; set; }
        public string Formation { get; set; }
    }

    public class MovementOrderRequest
    {
        public Guid? UnitDeploymentId { get; set; }
        public Position EndPosition { get; set; }
        public List<Position> Waypoints { get; set; }
        public string MovementType { get; set; }
        public string TerrainType { get; set; }
    }

    public class ExecuteMovementRequest
    {
        public string MovementOrderId { get; set; }
    }

    public class InitiateBattleRequest
    {
        public Guid? ScenarioId { get; set; }
        public string BattleName { get; set; }
        public Position BattleLocation { get; set; }
        public string BattleType { get; set; }
        public string TerrainType { get; set; }
        public string WeatherConditions { get; set; }
        public List<string> ParticipatingUnits { get; set; }
    }

    public class ResolveBattleRequest
    {
        public string BattleId { get; set; }
    }

    public class Position
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class AdvanceTurnRequest
    {
        public string ScenarioId { get; set; }
        public double SupplyDegradationRate { get; set; } = 5.0;
        public int CurrentTurn { get; set; } = 1;
    }

    public class ExecuteAttackOrderRequest
    {
        public Guid OrderId { get; set; }
    }

    public class AttackExecutionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Casualties { get; set; }
    }

    public class UpdateSupplyRequest
    {
        public string UnitDeploymentId { get; set; }
        public int SupplyState { get; set; }
    }

    public class CalculateMovementRequest
    {
        public string UnitDeploymentId { get; set; }
        public string TerrainType { get; set; }
        public double Distance { get; set; }
    }
}
