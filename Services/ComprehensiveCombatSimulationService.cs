using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TechWebSol.Data;
using TechWebSol.Models;

namespace TechWebSol.Services
{
    public interface IComprehensiveCombatSimulationService
    {
        Task<ComprehensiveSimulationResult> SimulateAttackDefenseAsync(Guid attackerTokenId, Guid targetTokenId);
    }

    public class ComprehensiveCombatSimulationService : IComprehensiveCombatSimulationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDetectionService _detectionService;
        private readonly CombatService _combatService;
        private readonly IMovementService _movementService;
        private readonly IWeaponEffectivenessService _weaponEffectiveness;
        private readonly IUnitCombatCalculatorService _unitCombatCalculator;
        private readonly ILogger<ComprehensiveCombatSimulationService> _logger;

        public ComprehensiveCombatSimulationService(
            ApplicationDbContext context,
            IDetectionService detectionService,
            CombatService combatService,
            IMovementService movementService,
            IWeaponEffectivenessService weaponEffectiveness,
            IUnitCombatCalculatorService unitCombatCalculator,
            ILogger<ComprehensiveCombatSimulationService> logger)
        {
            _context = context;
            _detectionService = detectionService;
            _combatService = combatService;
            _movementService = movementService;
            _weaponEffectiveness = weaponEffectiveness;
            _unitCombatCalculator = unitCombatCalculator;
            _logger = logger;
        }

        /// <summary>
        /// Check if victory conditions are met
        /// </summary>
        private (bool IsDecisive, string Outcome, string Reason) CheckVictoryConditions(
            int attackerCasualties, 
            int defenderCasualties, 
            int totalTimeMinutes,
            int combatRound)
        {
            // Defender Routed - Attacker Victory
            if (defenderCasualties >= 40)
            {
                return (true, "Attacker Victory", $"Defender routed ({defenderCasualties}% casualties)");
            }

            // Defender Combat Ineffective - Attacker Victory
            if (defenderCasualties >= 30)
            {
                return (true, "Attacker Victory", $"Defender combat ineffective ({defenderCasualties}% casualties)");
            }

            // Attacker Repelled - Defender Victory
            if (attackerCasualties >= 35)
            {
                return (true, "Defender Victory", $"Attacker forced to withdraw ({attackerCasualties}% casualties)");
            }

            // Attacker Stalled - Defender Victory
            if (attackerCasualties >= 25 && combatRound > 2)
            {
                return (true, "Defender Victory", $"Attacker unable to achieve breakthrough ({attackerCasualties}% casualties after {combatRound} rounds)");
            }

            // Time Limit Exceeded (24 hours) - Defender Victory
            if (totalTimeMinutes >= 1440) // 24 hours
            {
                if (defenderCasualties < attackerCasualties)
                {
                    return (true, "Defender Victory", $"Time limit exceeded - Defender held position (Att: {attackerCasualties}%, Def: {defenderCasualties}%)");
                }
                else
                {
                    return (true, "Stalemate", $"Time limit exceeded - Inconclusive (Att: {attackerCasualties}%, Def: {defenderCasualties}%)");
                }
            }

            // Maximum Combat Rounds (10) - Extended engagement
            if (combatRound >= 10)
            {
                if (attackerCasualties > defenderCasualties + 5)
                {
                    return (true, "Defender Victory", $"Attacker exhausted after {combatRound} rounds (Att: {attackerCasualties}%, Def: {defenderCasualties}%)");
                }
                else if (defenderCasualties > attackerCasualties + 5)
                {
                    return (true, "Attacker Victory", $"Defender exhausted after {combatRound} rounds (Att: {attackerCasualties}%, Def: {defenderCasualties}%)");
                }
                else
                {
                    return (true, "Stalemate", $"Inconclusive after {combatRound} rounds (Att: {attackerCasualties}%, Def: {defenderCasualties}%)");
                }
            }

            // Continue fighting
            return (false, string.Empty, string.Empty);
        }

        /// <summary>
        /// Comprehensive attack and defense simulation with iterative combat resolution
        /// </summary>
        public async Task<ComprehensiveSimulationResult> SimulateAttackDefenseAsync(Guid attackerTokenId, Guid targetTokenId)
        {
            try
            {
                var result = new ComprehensiveSimulationResult
                {
                    SimulationTime = DateTime.UtcNow
                };

                // ========================================
                // STEP 1: Resolve Suspected Token to Real Token
                // ========================================

                // Get attacker token
                var attackerToken = await _context.Tokens
                    .Include(t => t.MapMarkers.Where(m => m.IsActive))
                    .FirstOrDefaultAsync(t => t.Id == attackerTokenId);

                if (attackerToken == null)
                {
                    result.Success = false;
                    result.Message = "Attacker token not found";
                    return result;
                }

                var realDefenderToken = await _context.Tokens
                    .Include(t => t.MapMarkers.Where(m => m.IsActive))
                    .FirstOrDefaultAsync(t => t.Id == targetTokenId);

                // Get Brigades (contains all weapon data)
                var attackerBrigade = await _context.Brigades
                    .Include(b => b.InfantryBattalions)
                    .Include(b => b.ArmouredRegiments)
                    .Include(b => b.ArtilleryRegiments)
                    .Include(b => b.CombatEngineeringCompanies)
                    .FirstOrDefaultAsync(b => b.TokenId == attackerTokenId);

                // Initialize empty brigade if none exists - MUST have all collections initialized
                if (attackerBrigade == null)
                {
                    _logger.LogInformation($"⚠️ No brigade found for attacker token {attackerTokenId}, creating temporary brigade for direct units");
                    attackerBrigade = new Brigade
                    {
                        Id = Guid.NewGuid(), // Generate ID for temporary brigade
                        TokenId = attackerTokenId,
                        TeamId = attackerToken.TeamId,
                        ForceType = attackerToken.ForceType,
                        BrigadeCode = "DIRECT",
                        IsActive = true,
                        InfantryBattalions = new List<InfantryBattalion>(),
                        ArmouredRegiments = new List<ArmouredRegiment>(),
                        ArtilleryRegiments = new List<ArtilleryRegiment>(),
                        CombatEngineeringCompanies = new List<CombatEngineeringCompany>()
                    };
                }

                // Load ALL units for this token (both brigade units and direct units)
                // This ensures direct units (without BrigadeId) are included
                attackerBrigade.InfantryBattalions = await _context.InfantryBattalions
                    .Where(i => i.TokenId == attackerTokenId && i.IsActive)
                    .OrderByDescending(i => i.CreatedDate)
                    .ToListAsync();

                attackerBrigade.ArmouredRegiments = await _context.ArmouredRegiments
                    .Where(i => i.TokenId == attackerTokenId && i.IsActive)
                    .ToListAsync();

                attackerBrigade.ArtilleryRegiments = await _context.ArtilleryRegiments
                    .Where(i => i.TokenId == attackerTokenId && i.IsActive)
                    .ToListAsync();

                attackerBrigade.CombatEngineeringCompanies = await _context.CombatEngineeringCompanies
                    .Where(i => i.TokenId == attackerTokenId && i.IsActive)
                    .OrderByDescending(i => i.CreatedDate)
                    .ToListAsync();
                    
                _logger.LogInformation($"✅ Loaded attacker units: {attackerBrigade.InfantryBattalions.Count} infantry, {attackerBrigade.ArmouredRegiments.Count} armour, {attackerBrigade.ArtilleryRegiments.Count} artillery, {attackerBrigade.CombatEngineeringCompanies.Count} engineers");

                // Get Brigades (contains all weapon data)
                var defenderBrigade = await _context.Brigades
                    .Include(b => b.InfantryBattalions)
                    .Include(b => b.ArmouredRegiments)
                    .Include(b => b.ArtilleryRegiments)
                    .Include(b => b.CombatEngineeringCompanies)
                    .FirstOrDefaultAsync(b => b.TokenId == realDefenderToken.Id);

                // Initialize empty brigade if none exists - MUST have all collections initialized
                if (defenderBrigade == null)
                {
                    _logger.LogInformation($"⚠️ No brigade found for defender token {realDefenderToken.Id}, creating temporary brigade for direct units");
                    defenderBrigade = new Brigade
                    {
                        Id = Guid.NewGuid(), // Generate ID for temporary brigade
                        TokenId = realDefenderToken.Id,
                        TeamId = realDefenderToken.TeamId,
                        ForceType = realDefenderToken.ForceType,
                        BrigadeCode = "DIRECT",
                        IsActive = true,
                        InfantryBattalions = new List<InfantryBattalion>(),
                        ArmouredRegiments = new List<ArmouredRegiment>(),
                        ArtilleryRegiments = new List<ArtilleryRegiment>(),
                        CombatEngineeringCompanies = new List<CombatEngineeringCompany>()
                    };
                }

                // Load ALL units for this token (both brigade units and direct units)
                // This ensures direct units (without BrigadeId) are included
                defenderBrigade.InfantryBattalions = await _context.InfantryBattalions
                    .Where(i => i.TokenId == realDefenderToken.Id && i.IsActive)
                    .OrderByDescending(i => i.CreatedDate)
                    .ToListAsync();

                defenderBrigade.ArmouredRegiments = await _context.ArmouredRegiments
                    .Where(i => i.TokenId == realDefenderToken.Id && i.IsActive)
                    .ToListAsync();

                defenderBrigade.ArtilleryRegiments = await _context.ArtilleryRegiments
                    .Where(i => i.TokenId == realDefenderToken.Id && i.IsActive)
                    .ToListAsync();

                defenderBrigade.CombatEngineeringCompanies = await _context.CombatEngineeringCompanies
                    .Where(i => i.TokenId == realDefenderToken.Id && i.IsActive)
                    .OrderByDescending(i => i.CreatedDate)
                    .ToListAsync();
                    
                _logger.LogInformation($"✅ Loaded defender units: {defenderBrigade.InfantryBattalions.Count} infantry, {defenderBrigade.ArmouredRegiments.Count} armour, {defenderBrigade.ArtilleryRegiments.Count} artillery, {defenderBrigade.CombatEngineeringCompanies.Count} engineers");


                _logger.LogInformation("🎯 SIMULATION START: Attacker={AttackerName} (Brigade={AttackerBrigade}), Defender={DefenderName} (Brigade={DefenderBrigade})", 
                    attackerToken.Name, 
                    attackerBrigade?.Id, 
                    realDefenderToken.Name, 
                    defenderBrigade?.Id);

                // Store token information
                result.AttackerTokenName = attackerToken.Name;
                result.DefenderTokenName = realDefenderToken.Name;
                result.DetectionConfidence = 100;

                // ========================================
                // ITERATIVE COMBAT RESOLUTION LOOP
                // Fight until decisive victory or time limit
                // ========================================
                
                int cumulativeAttackerCasualties = 0;
                int cumulativeDefenderCasualties = 0;
                int totalEngagementTime = 0;
                int combatRound = 0;

                // Track remaining strength for each round (starts at 100%)
                int attackerRemainingStrength = 100;
                int defenderRemainingStrength = 100;

                _logger.LogInformation("🔄 ITERATIVE COMBAT ENGINE START - Fighting until objectives achieved or forces exhausted");

                while (true)
                {
                    combatRound++;
                    _logger.LogInformation("⚔️ === COMBAT ROUND {Round} === Attacker: {AttStr}%, Defender: {DefStr}%", 
                        combatRound, attackerRemainingStrength, defenderRemainingStrength);

                    // ========================================
                    // ATTACK SIMULATION FOR THIS ROUND
                    // ========================================
                    var attackPhases = await SimulateAttackPhasesAsync(
                        attackerToken,
                        realDefenderToken,
                        attackerBrigade,
                        defenderBrigade,
                        100);

                    // ========================================
                    // DEFENSE SIMULATION FOR THIS ROUND
                    // ========================================
                    var defensePhases = await SimulateDefensePhasesAsync(
                        realDefenderToken,
                        attackerToken,
                        defenderBrigade,
                        attackerBrigade);

                    // Calculate casualties for this round
                    int roundAttackerCasualties = attackPhases.Sum(p => p.CasualtiesAttacker);
                    int roundDefenderCasualties = attackPhases.Sum(p => p.CasualtiesDefender) + 
                                                 defensePhases.Sum(p => p.CasualtiesDefender);

                    // Add round casualties to cumulative total
                    cumulativeAttackerCasualties += roundAttackerCasualties;
                    cumulativeDefenderCasualties += roundDefenderCasualties;

                    // Update remaining strength (apply casualties to remaining force)
                    attackerRemainingStrength = Math.Max(0, attackerRemainingStrength - roundAttackerCasualties);
                    defenderRemainingStrength = Math.Max(0, defenderRemainingStrength - roundDefenderCasualties);

                    // Calculate time for this round
                    int roundTime = attackPhases.Sum(p => p.DelayMinutes) + 
                                   defensePhases.Sum(p => p.TimeToStayMinutes + p.MovementDelayMinutes + p.CounterAttackDelayMinutes);
                    totalEngagementTime += roundTime;

                    // Add phases to result with round marker
                    result.AttackPhases.Add(new AttackPhaseResult
                    {
                        PhaseName = $"=== ROUND {combatRound} ===",
                        PhaseType = "Marker",
                        Notes = $"Cumulative - Attacker: {cumulativeAttackerCasualties}%, Defender: {cumulativeDefenderCasualties}%"
                    });
                    result.AttackPhases.AddRange(attackPhases);
                    result.DefensePhases.AddRange(defensePhases);

                    _logger.LogInformation("📊 Round {Round} Results: Att casualties {AttRound}% (cumulative {AttTotal}%), Def casualties {DefRound}% (cumulative {DefTotal}%), Time: {Time}min (total {TotalTime}min)", 
                        combatRound, roundAttackerCasualties, cumulativeAttackerCasualties, 
                        roundDefenderCasualties, cumulativeDefenderCasualties, 
                        roundTime, totalEngagementTime);

                    // ========================================
                    // CHECK VICTORY CONDITIONS
                    // ========================================
                    var (isDecisive, outcome, reason) = CheckVictoryConditions(
                        cumulativeAttackerCasualties, 
                        cumulativeDefenderCasualties, 
                        totalEngagementTime,
                        combatRound);

                    if (isDecisive)
                    {
                        result.VictoryOutcome = outcome;
                        result.VictoryReason = reason;
                        result.TotalCombatRounds = combatRound;
                        result.TotalEngagementTimeMinutes = totalEngagementTime;
                        result.FinalAttackerCasualtiesPercent = cumulativeAttackerCasualties;
                        result.FinalDefenderCasualtiesPercent = cumulativeDefenderCasualties;

                        _logger.LogInformation("🏆 DECISIVE OUTCOME: {Outcome} - {Reason}", outcome, reason);
                        break; // Exit combat loop
                    }

                    _logger.LogInformation("⚔️ Combat continues - No decisive outcome yet");
                }

                // ========================================
                // STEP 4: Generate Summary
                // ========================================
                result.AttackSummary = GenerateAttackSummary(result.AttackPhases);
                result.DefenseSummary = GenerateDefenseSummary(result.DefensePhases);

                // Update summaries with cumulative totals
                result.AttackSummary.TotalAttackerCasualties = cumulativeAttackerCasualties;
                result.AttackSummary.TotalDefenderCasualties = cumulativeDefenderCasualties;
                result.AttackSummary.TotalDelayMinutes = totalEngagementTime;

                result.Success = true;
                result.Message = $"Simulation completed: {result.VictoryOutcome}";

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in comprehensive combat simulation");
                return new ComprehensiveSimulationResult
                {
                    Success = false,
                    Message = $"Simulation error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Simulate attack phases through engagement zones and defense locations
        /// </summary>
        private async Task<List<AttackPhaseResult>> SimulateAttackPhasesAsync(
            Token attacker,
            Token defender,
            Brigade? attackerBrigade,
            Brigade? defenderBrigade,
            double detectionConfidence)
        {
            var phases = new List<AttackPhaseResult>();

            // Get defense elements in the area
            var defenseElements = await _context.DefenseElements
                .Where(de => de.TokenId == defender.Id && de.IsActive)
                .ToListAsync();

            // Get attacker and defender positions
            var attackerMarker = attacker.MapMarkers?.FirstOrDefault(m => m.IsActive);
            var defenderMarker = defender.MapMarkers?.FirstOrDefault(m => m.IsActive);

            if (attackerMarker == null || defenderMarker == null)
            {
                return phases;
            }

            var attackerLat = double.Parse(attackerMarker.latitude);
            var attackerLng = double.Parse(attackerMarker.longitude);
            var defenderLat = double.Parse(defenderMarker.latitude);
            var defenderLng = double.Parse(defenderMarker.longitude);

            // ========================================
            // PHASE 0: Pre-Attack Preparation (2-3 hours)
            // ========================================
            var prepPhase = new AttackPhaseResult
            {
                PhaseName = "Pre-Attack Preparation",
                PhaseType = "Preparation"
            };

            // Artillery preparation, reconnaissance, briefings, assembly
            int prepDelay = 60; // Base 60 min for hasty attack
            prepDelay += 30; // Reconnaissance
            prepDelay += 20; // Final briefings
            prepDelay += 30; // Assembly into attack formation
            
            // Add artillery prep time if attacker has artillery/fires
            var attackerArtillery = attackerBrigade?.ArtilleryRegiments?.Count ?? 0;
            if (attackerArtillery > 0) // Has artillery support
            {
                prepDelay += 60; // 1 hour artillery prep
            }
            
            prepPhase.DelayMinutes = prepDelay;
            prepPhase.CasualtiesAttacker = 0;
            prepPhase.CasualtiesDefender = 0; // Minimal casualties from prep fires
            prepPhase.Notes = $"Artillery prep, reconnaissance, assembly: {prepDelay} min";

            phases.Add(prepPhase);

            // ========================================
            // PHASE 1: Movement to Contact (Approach)
            // ========================================
            var approachPhase = new AttackPhaseResult
            {
                PhaseName = "Approach Movement",
                PhaseType = "Movement"
            };

            var distanceKm = CalculateDistance(attackerLat, attackerLng, defenderLat, defenderLng);
            var movementSpeed = 30; // km/turn (standard brigade road march speed)
            
            // Tactical movement is 50% slower than road march
            var tacticalSpeed = movementSpeed * 0.5;
            
            // Base movement time in tactical formation
            int baseMovementTime = (int)((distanceKm / tacticalSpeed) * 60); // 60 min per turn (tactical)
            
            // Apply terrain penalties (default: open terrain)
            var terrainMultiplier = GetTerrainMovementMultiplier("open");
            baseMovementTime = (int)(baseMovementTime * terrainMultiplier);
            
            // Apply weather/visibility delays
            baseMovementTime = (int)(baseMovementTime * 1.2); // Assume limited visibility adds 20%
            
            // Add security halts and regrouping
            int securityHalts = Math.Max(1, (int)(distanceKm / 5)); // 1 halt per 5km
            baseMovementTime += securityHalts * 15; // 15 min per halt
            
            approachPhase.DelayMinutes = baseMovementTime;
            approachPhase.CasualtiesAttacker = 0; // No casualties during approach
            approachPhase.CasualtiesDefender = 0;
            approachPhase.Notes = $"Tactical movement: {distanceKm:F2} km, Terrain: open, Halts: {securityHalts}";

            phases.Add(approachPhase);

            // ========================================
            // PHASE 2: Engagement through Kill Zones
            // ========================================
            var killZones = defenseElements.Where(de => de.Category.ToLower() == "killzone").ToList();
            
            if (killZones.Any())
            {
                foreach (var killZone in killZones)
                {
                    var killZonePhase = await SimulateKillZoneEngagementAsync(
                        attackerBrigade,
                        defenderBrigade,
                        killZone,
                        detectionConfidence);
                    
                    phases.Add(killZonePhase);
                }
            }
            else
            {
                // No kill zones defined - add placeholder indicating this
                _logger.LogInformation("No kill zones found for defender {DefenderId}", defender.Id);
            }

            // ========================================
            // PHASE 3: Engagement through Minefields
            // ========================================
            var minefields = defenseElements.Where(de => de.Category.ToLower() == "minefield").ToList();
            
            if (minefields.Any())
            {
                foreach (var minefield in minefields)
                {
                    var minefieldPhase = await SimulateMinefieldEngagementAsync(
                        attackerBrigade,
                        defenderBrigade,
                        minefield,
                        detectionConfidence);
                    
                    phases.Add(minefieldPhase);
                }
            }

            // ========================================
            // PHASE 4: Engagement through Obstacles
            // ========================================
            var obstacles = defenseElements.Where(de => de.Category.ToLower() == "obstacle").ToList();
            
            if (obstacles.Any())
            {
                foreach (var obstacle in obstacles)
                {
                    var obstaclePhase = await SimulateObstacleEngagementAsync(
                        attackerBrigade,
                        defenderBrigade,
                        obstacle,
                        detectionConfidence);
                    
                    phases.Add(obstaclePhase);
                }
            }

            // ========================================
            // PHASE 5: Assault on Defense Positions
            // ========================================
            var defensePositions = defenseElements.Where(de => de.Category.ToLower() == "position").ToList();
            
            if (defensePositions.Any())
            {
                foreach (var position in defensePositions)
                {
                    var positionPhase = await SimulateDefensePositionAssaultAsync(
                        attackerBrigade,
                        defenderBrigade,
                        position,
                        detectionConfidence);
                    
                    phases.Add(positionPhase);
                }
            }
            else
            {
                // No prepared positions - direct assault
                var directAssaultPhase = await SimulateDirectAssaultAsync(
                    attackerBrigade,
                    defenderBrigade,
                    100);
                
                phases.Add(directAssaultPhase);
            }

            return phases;
        }

        /// <summary>
        /// Simulate defense phases and counter-attack
        /// </summary>
        private async Task<List<DefensePhaseResult>> SimulateDefensePhasesAsync(
            Token defender,
            Token attacker,
            Brigade? defenderBrigade,
            Brigade? attackerBrigade)
        {
            var phases = new List<DefensePhaseResult>();

            // ========================================
            // PHASE 1: Hold Position (Time to Stay)
            // ========================================
            var holdPhase = new DefensePhaseResult
            {
                PhaseName = "Hold Position",
                PhaseType = "Defense"
            };

            // Calculate REAL strength ratio from brigade composition
            var defenseStrength = CalculateBrigadeCombatPower(defenderBrigade);
            var attackStrength = CalculateBrigadeCombatPower(attackerBrigade);
            var ratio = defenseStrength / attackStrength;

            // Time to stay based on strength ratio
            if (ratio >= 1.5)
            {
                holdPhase.TimeToStayMinutes = 120; // Strong defense - 2 hours
            }
            else if (ratio >= 1.0)
            {
                holdPhase.TimeToStayMinutes = 60; // Equal - 1 hour
            }
            else if (ratio >= 0.5)
            {
                holdPhase.TimeToStayMinutes = 30; // Weak - 30 minutes
            }
            else
            {
                holdPhase.TimeToStayMinutes = 15; // Very weak - 15 minutes
            }

            holdPhase.Notes = $"Defense strength ratio: {ratio:F2}:1";
            phases.Add(holdPhase);

            // ========================================
            // PHASE 2: Movement to Counter-Penetration Position
            // ========================================
            var repositionPhase = new DefensePhaseResult
            {
                PhaseName = "Reposition to Counter-Penetration",
                PhaseType = "Movement"
            };

            // Assume counter-penetration position is 2-5km away
            var repositionDistance = 3.0; // km
            var defenseSpeed = 25; // Standard brigade speed
            repositionPhase.MovementDelayMinutes = (int)((repositionDistance / defenseSpeed) * 30);
            repositionPhase.CasualtiesDefender = 0; // Minimal casualties during tactical movement
            repositionPhase.CasualtiesAttacker = 0;
            repositionPhase.Notes = $"Tactical repositioning: {repositionDistance:F1} km";

            phases.Add(repositionPhase);

            // ========================================
            // PHASE 3: Counter-Attack
            // ========================================
            var counterAttackPhase = await SimulateCounterAttackAsync(
                defenderBrigade,
                attackerBrigade);

            phases.Add(counterAttackPhase);

            return phases;
        }

        /// <summary>
        /// Simulate engagement through a kill zone
        /// </summary>
        private async Task<AttackPhaseResult> SimulateKillZoneEngagementAsync(
            Brigade? attackerBrigade,
            Brigade? defenderBrigade,
            DefenseElement killZone,
            double detectionConfidence)
        {
            var phase = new AttackPhaseResult
            {
                PhaseName = $"Kill Zone: {killZone.Type}",
                PhaseType = "Engagement",
                Location = killZone.Coordinates
            };

            var killZoneEffectiveness = killZone.Effectiveness * (killZone.Strength / 100.0);
            
            // Get morale and fatigue from deployment
            // Brigade model doesn't store morale/fatigue - use standard combat-ready values
            // TODO: Add morale/fatigue tracking at Brigade or Token level for future enhancements
            var attackerMorale = 100;  // 100 = fully motivated, combat-ready troops
            var attackerFatigue = 0;   // 0 = fully rested troops
            
            // Casualties calculation
            var baseAttackerCasualties = 5.0 + (killZoneEffectiveness * 10.0); // 5-15%
            var baseDefenderCasualties = 1.0; // Minimal defender casualties in prepared kill zone

            // Adjust for detection confidence (fog of war)
            if (detectionConfidence < 0.5)
            {
                baseAttackerCasualties *= 1.5; // 50% more casualties with poor intelligence
            }

            // Adjust for morale (low morale = higher casualties)
            if (attackerMorale < 50)
            {
                baseAttackerCasualties *= 1.3; // +30% casualties with low morale
            }

            // Adjust for fatigue (exhausted troops take more casualties)
            if (attackerFatigue > 70)
            {
                baseAttackerCasualties *= 1.2; // +20% casualties when exhausted
            }

            phase.CasualtiesAttacker = (int)(baseAttackerCasualties);
            phase.CasualtiesDefender = (int)(baseDefenderCasualties);
            
            // Delay calculation (time to suppress/bypass kill zone)
            // Includes: reconnaissance (15min), suppression fire (20-40min), movement through zone (30-120min), reorganization (15-30min)
            var baseDelay = 30 + (killZoneEffectiveness * 180); // 30-210 minutes
            baseDelay += 20; // Add suppression fire time
            baseDelay += 15; // Add reorganization after kill zone
            baseDelay += 10; // Add command decision time
            
            // Fatigue increases delay
            if (attackerFatigue > 60)
            {
                baseDelay *= 1.3; // 30% slower when fatigued
            }
            
            // Poor detection increases caution/delay
            if (detectionConfidence < 0.5)
            {
                baseDelay *= 1.2; // 20% slower with poor intelligence
            }

            phase.DelayMinutes = (int)baseDelay;

            phase.Notes = $"Effectiveness: {killZoneEffectiveness:F2}, Detection: {detectionConfidence:P0}, Morale: {attackerMorale}, Fatigue: {attackerFatigue}";

            return phase;
        }

        /// <summary>
        /// Simulate engagement through a minefield
        /// </summary>
        private async Task<AttackPhaseResult> SimulateMinefieldEngagementAsync(
            Brigade? attackerBrigade,
            Brigade? defenderBrigade,
            DefenseElement minefield,
            double detectionConfidence)
        {
            var phase = new AttackPhaseResult
            {
                PhaseName = $"Minefield: {minefield.Type}",
                PhaseType = "Engagement",
                Location = minefield.Coordinates
            };

            var minefieldEffectiveness = minefield.Effectiveness * (minefield.Strength / 100.0);
            
            // Get morale and fatigue from deployment
            // Brigade model doesn't store morale/fatigue - use standard combat-ready values
            // TODO: Add morale/fatigue tracking at Brigade or Token level for future enhancements
            var attackerMorale = 100;  // 100 = fully motivated, combat-ready troops
            var attackerFatigue = 0;   // 0 = fully rested troops
            
            // Casualties calculation - minefields cause casualties and delay
            var baseAttackerCasualties = 8.0 + (minefieldEffectiveness * 12.0); // 8-20% casualties
            var baseDefenderCasualties = 0.0; // No defender casualties from minefields

            // Adjust for detection confidence (if poorly detected, more casualties)
            if (detectionConfidence < 0.5)
            {
                baseAttackerCasualties *= 1.6; // 60% more casualties with poor intelligence
                phase.Notes = "Poor intelligence led to increased minefield casualties. ";
            }

            // Engineers can reduce casualties
            var hasEngineers = attackerBrigade?.CombatEngineeringCompanies?.Any() ?? false;
            if (hasEngineers)
            {
                baseAttackerCasualties *= 0.5; // Engineers reduce casualties by 50%
                phase.Notes += "Engineer support reduced minefield casualties. ";
            }

            phase.CasualtiesAttacker = (int)(baseAttackerCasualties);
            phase.CasualtiesDefender = (int)(baseDefenderCasualties);
            
            // Delay calculation (time to breach/clear minefield)
            // Includes: probing/detection (20-30min), marking lanes (15-20min), breaching (60-180min), passage of lines (20-30min)
            var baseDelay = 60 + (minefieldEffectiveness * 180); // 60-240 minutes
            baseDelay += 25; // Add probing and marking time
            baseDelay += 20; // Add passage of lines coordination
            baseDelay += 10; // Add resupply time after breaching
            
            // Engineers speed up breaching significantly
            if (hasEngineers)
            {
                baseDelay *= 0.5; // Engineers reduce time by 50%
            }
            else
            {
                baseDelay *= 1.4; // Without engineers, much slower and more cautious
            }
            
            // Fatigue increases delay
            if (attackerFatigue > 60)
            {
                baseDelay *= 1.4; // 40% slower when fatigued (dangerous work)
            }
            
            // Poor detection = slower, more cautious
            if (detectionConfidence < 0.5)
            {
                baseDelay *= 1.3; // 30% slower with poor intelligence
            }

            phase.DelayMinutes = (int)baseDelay;

            if (string.IsNullOrEmpty(phase.Notes))
            {
                phase.Notes = $"Effectiveness: {minefieldEffectiveness:F2}, Detection: {detectionConfidence:P0}";
            }

            return phase;
        }

        /// <summary>
        /// Simulate engagement through obstacles
        /// </summary>
        private async Task<AttackPhaseResult> SimulateObstacleEngagementAsync(
            Brigade? attackerBrigade,
            Brigade? defenderBrigade,
            DefenseElement obstacle,
            double detectionConfidence)
        {
            var phase = new AttackPhaseResult
            {
                PhaseName = $"Obstacle: {obstacle.Type}",
                PhaseType = "Engagement",
                Location = obstacle.Coordinates
            };

            var obstacleEffectiveness = obstacle.Effectiveness * (obstacle.Strength / 100.0);
            
            // Get morale and fatigue from deployment
            // Brigade model doesn't store morale/fatigue - use standard combat-ready values
            // TODO: Add morale/fatigue tracking at Brigade or Token level for future enhancements
            var attackerMorale = 100;  // 100 = fully motivated, combat-ready troops
            var attackerFatigue = 0;   // 0 = fully rested troops
            
            // Obstacles primarily cause delay, minimal casualties
            var baseAttackerCasualties = 2.0 + (obstacleEffectiveness * 5.0); // 2-7% casualties
            var baseDefenderCasualties = 1.0; // Minimal defender casualties

            // Adjust for morale (low morale = higher casualties while overcoming obstacles)
            if (attackerMorale < 50)
            {
                baseAttackerCasualties *= 1.4; // +40% casualties with low morale
            }

            // Engineers reduce casualties
            var hasEngineers = attackerBrigade?.CombatEngineeringCompanies?.Any() ?? false;
            if (hasEngineers)
            {
                baseAttackerCasualties *= 0.3; // Engineers significantly reduce casualties
                phase.Notes = "Engineer support minimized obstacle casualties. ";
            }

            phase.CasualtiesAttacker = (int)(baseAttackerCasualties);
            phase.CasualtiesDefender = (int)(baseDefenderCasualties);
            
            // Delay calculation (time to breach/bypass obstacle)
            var baseDelay = 20 + (obstacleEffectiveness * 40); // 20-60 minutes
            
            // Engineers speed up breaching
            if (hasEngineers)
            {
                baseDelay *= 0.4; // Engineers reduce time by 60%
            }
            else
            {
                baseDelay *= 1.5; // Without engineers, obstacles take 50% longer
            }
            
            // Fatigue increases delay
            if (attackerFatigue > 60)
            {
                baseDelay *= 1.25; // 25% slower when fatigued
            }

            phase.DelayMinutes = (int)baseDelay;

            if (string.IsNullOrEmpty(phase.Notes))
            {
                phase.Notes = $"Effectiveness: {obstacleEffectiveness:F2}, Morale: {attackerMorale}, Fatigue: {attackerFatigue}";
            }

            return phase;
        }

        /// <summary>
        /// Simulate assault on a prepared defense position
        /// NOW USES WEAPON-LEVEL CALCULATIONS
        /// </summary>
        private async Task<AttackPhaseResult> SimulateDefensePositionAssaultAsync(
            Brigade? attackerBrigade,
            Brigade? defenderBrigade,
            DefenseElement position,
            double detectionConfidence)
        {
            var phase = new AttackPhaseResult
            {
                PhaseName = $"Assault: {position.Type} Position",
                PhaseType = "Combat",
                Location = position.Coordinates
            };

            // Validate brigades - should never be null since we create temporary ones
            if (attackerBrigade == null || defenderBrigade == null)
            {
                _logger.LogError("❌ CRITICAL: Brigade is null - Attacker: {AttackerNull}, Defender: {DefenderNull}", 
                    attackerBrigade == null, 
                    defenderBrigade == null);
                throw new InvalidOperationException("Brigade data missing for defense position assault.");
            }
            
            // Validate TokenId is set
            if (!attackerBrigade.TokenId.HasValue || !defenderBrigade.TokenId.HasValue)
            {
                _logger.LogError("❌ CRITICAL: TokenId not set on brigade - AttackerTokenId: {AttackerTokenId}, DefenderTokenId: {DefenderTokenId}", 
                    attackerBrigade.TokenId, 
                    defenderBrigade.TokenId);
                throw new InvalidOperationException("TokenId not found for defense position assault. Cannot calculate weapon effects.");
            }
            
            _logger.LogInformation("✅ DEFENSE ASSAULT: Attacker TokenId={AttackerTokenId}, Defender TokenId={DefenderTokenId}", attackerBrigade.TokenId, defenderBrigade.TokenId);

            // Get tokens to extract real terrain data
            var attackerToken = await _context.Tokens
                .Include(t => t.MapMarkers.Where(m => m.IsActive))
                .FirstOrDefaultAsync(t => t.Id == attackerBrigade.TokenId.Value);
            
            var defenderToken = await _context.Tokens
                .Include(t => t.MapMarkers.Where(m => m.IsActive))
                .FirstOrDefaultAsync(t => t.Id == defenderBrigade.TokenId.Value);

            // Get REAL terrain from map data
            var terrain = defenderToken != null 
                ? await GetTerrainAtTokenPositionAsync(defenderToken) 
                : "open";

            // Create combat context with REAL terrain data
            var context = new CombatContext
            {
                Terrain = terrain, // REAL terrain from map data
                Weather = "clear", // TODO: Get from game state/scenario settings
                Visibility = "day clear", // TODO: Get from game time/weather system
                DefenderProtection = "improved", // Prepared positions
                AttackerPosture = "moving slow",
                DefenderPosture = "static"
            };

            // Apply fortification effectiveness to protection level
            if (position.Effectiveness >= 0.8)
            {
                context.DefenderProtection = "fortified";
            }
            else if (position.Effectiveness >= 0.6)
            {
                context.DefenderProtection = "improved";
            }
            else
            {
                context.DefenderProtection = "hasty";
            }

            // Calculate weapon-level combat using TokenId directly
            var combatResult = await _unitCombatCalculator.CalculateUnitVsUnitCombatAsync(
                attackerBrigade.TokenId.Value,
                defenderBrigade.TokenId.Value,
                context);

            if (!combatResult.Success)
            {
                _logger.LogError("❌ WEAPON COMBAT CALCULATION FAILED: {ErrorMessage}", combatResult.ErrorMessage);
                throw new InvalidOperationException($"Weapon combat calculation failed: {combatResult.ErrorMessage}");
            }
            
            // Convert weapon casualties to percentages using REAL brigade strength
            var attackerStrength = CalculateBrigadeStrength(attackerBrigade);
            var defenderStrength = CalculateBrigadeStrength(defenderBrigade);

            // CRITICAL: Model SUSTAINED COMBAT over the entire engagement duration
            // A 3-hour assault isn't one volley - it's sustained combat with multiple engagements
            // Calculate engagement intensity: prep fires (30min) + main assault (1-4 hours) + consolidation (30min)
            var combatDurationMinutes = phase.DelayMinutes;
            var sustainedCombatMultiplier = Math.Max(1.0, combatDurationMinutes / 30.0); // Every 30 min = another engagement cycle
            
            // Apply multiplier to represent sustained combat
            var sustainedAttackerCasualties = combatResult.TotalDefenderCasualtiesInflicted * sustainedCombatMultiplier;
            var sustainedDefenderCasualties = combatResult.TotalAttackerCasualtiesInflicted * sustainedCombatMultiplier;

            // Calculate percentages from sustained combat
            var attackerCasualtyPct = (sustainedAttackerCasualties / attackerStrength) * 100.0;
            var defenderCasualtyPct = (sustainedDefenderCasualties / defenderStrength) * 100.0;

            // Apply doctrine-based bounds (assault on fortified position)
            phase.CasualtiesAttacker = Math.Min(45, Math.Max(5, (int)attackerCasualtyPct)); // Assault: 5-45% attacker casualties
            phase.CasualtiesDefender = Math.Min(60, Math.Max(8, (int)defenderCasualtyPct)); // Fortified defense: 8-60% defender casualties

            _logger.LogInformation("✅ DEFENSE POSITION ({Duration}min sustained): Single engagement: Att {AttRaw} / Def {DefRaw}. Sustained (x{Multiplier:F1}): Att {AttSustained:F0} / Def {DefSustained:F0}. Final: Att {AttPct:F2}% ({AttFinal}%) / Def {DefPct:F2}% ({DefFinal}%)", 
                combatDurationMinutes,
                combatResult.TotalDefenderCasualtiesInflicted, combatResult.TotalAttackerCasualtiesInflicted,
                sustainedCombatMultiplier,
                sustainedAttackerCasualties, sustainedDefenderCasualties,
                attackerCasualtyPct, phase.CasualtiesAttacker,
                defenderCasualtyPct, phase.CasualtiesDefender);

            // Calculate delay based on REAL force ratio from brigade composition
            var attackerPower = CalculateBrigadeCombatPower(attackerBrigade);
            var defenderPower = CalculateBrigadeCombatPower(defenderBrigade);
            var ratio = attackerPower / (defenderPower * position.Effectiveness);
            
            if (ratio >= 3.0)
            {
                phase.DelayMinutes = 120; // 2 hours
            }
            else if (ratio >= 1.5)
            {
                phase.DelayMinutes = 210; // 3.5 hours
            }
            else if (ratio >= 0.8)
            {
                phase.DelayMinutes = 300; // 5 hours
            }
            else
            {
                phase.DelayMinutes = 420; // 7 hours
            }

            // Add weapon engagement time
            phase.DelayMinutes += (combatResult.TotalEngagementTimeSeconds / 60); // Add weapon engagement time
            phase.DelayMinutes += 20; // Casualty evacuation
            phase.DelayMinutes += 15; // Ammunition resupply
            phase.DelayMinutes += 10; // Command coordination

            // Adjust for detection confidence
            if (detectionConfidence < 0.5)
            {
                phase.CasualtiesAttacker = (int)(phase.CasualtiesAttacker * 1.3);
                phase.DelayMinutes = (int)(phase.DelayMinutes * 1.3);
            }
            
            // Adjust for morale and fatigue
            // Brigade model doesn't store morale/fatigue - use standard combat-ready values
            // TODO: Add morale/fatigue tracking at Brigade or Token level for future enhancements
            var attackerMorale = 100;  // 100 = fully motivated, combat-ready troops
            var attackerFatigue = 0;   // 0 = fully rested troops
            
            if (attackerMorale < 50)
            {
                phase.CasualtiesAttacker = (int)(phase.CasualtiesAttacker * 1.15);
                phase.DelayMinutes = (int)(phase.DelayMinutes * 1.2);
            }
            
            if (attackerFatigue > 60)
            {
                phase.CasualtiesAttacker = (int)(phase.CasualtiesAttacker * 1.1);
                phase.DelayMinutes = (int)(phase.DelayMinutes * 1.15);
            }

            // Build detailed notes with weapon breakdown
            var weaponSummary = string.Join(", ", combatResult.AttackerWeaponResults
                .Where(w => w.TotalCasualties > 0.1)
                .Select(w => $"{w.WeaponType}({w.Quantity}): {w.TotalCasualties:F1} kills")
                .Take(3));

            phase.Notes = $"Force ratio: {ratio:F2}:1, Protection: {context.DefenderProtection}, Weapons: {weaponSummary}";

            return phase;
        }

        /// <summary>
        /// Simulate direct assault (no prepared positions)
        /// NOW USES WEAPON-LEVEL CALCULATIONS
        /// </summary>
        private async Task<AttackPhaseResult> SimulateDirectAssaultAsync(
            Brigade? attackerBrigade,
            Brigade? defenderBrigade,
            double detectionConfidence)
        {
            var phase = new AttackPhaseResult
            {
                PhaseName = "Direct Assault",
                PhaseType = "Combat"
            };

            // Validate brigades - should never be null since we create temporary ones
            if (attackerBrigade == null || defenderBrigade == null)
            {
                _logger.LogError("❌ CRITICAL: Brigade is null - Attacker: {AttackerNull}, Defender: {DefenderNull}", 
                    attackerBrigade == null, 
                    defenderBrigade == null);
                throw new InvalidOperationException("Brigade data missing for direct assault.");
            }
            
            // Validate TokenId is set
            if (!attackerBrigade.TokenId.HasValue || !defenderBrigade.TokenId.HasValue)
            {
                _logger.LogError("❌ CRITICAL: TokenId not set on brigade - AttackerTokenId: {AttackerTokenId}, DefenderTokenId: {DefenderTokenId}", 
                    attackerBrigade.TokenId, 
                    defenderBrigade.TokenId);
                throw new InvalidOperationException("TokenId not found for direct assault. Cannot calculate weapon effects.");
            }
            
            _logger.LogInformation("✅ DIRECT ASSAULT: Attacker TokenId={AttackerTokenId}, Defender TokenId={DefenderTokenId}", attackerBrigade.TokenId, defenderBrigade.TokenId);

            // Get tokens to extract real terrain data
            var attackerToken = await _context.Tokens
                .Include(t => t.MapMarkers.Where(m => m.IsActive))
                .FirstOrDefaultAsync(t => t.Id == attackerBrigade.TokenId.Value);
            
            var defenderToken = await _context.Tokens
                .Include(t => t.MapMarkers.Where(m => m.IsActive))
                .FirstOrDefaultAsync(t => t.Id == defenderBrigade.TokenId.Value);

            // Get REAL terrain from map data
            var terrain = defenderToken != null 
                ? await GetTerrainAtTokenPositionAsync(defenderToken) 
                : "open";

            // Create combat context with REAL terrain data
            var context = new CombatContext
            {
                Terrain = terrain, // REAL terrain from map data
                Weather = "clear", // TODO: Get from game state/scenario settings
                Visibility = "day clear", // TODO: Get from game time/weather system
                DefenderProtection = "none", // No prepared positions
                AttackerPosture = "moving slow",
                DefenderPosture = "static"
            };

            // Calculate weapon-level combat using TokenId directly
            var combatResult = await _unitCombatCalculator.CalculateUnitVsUnitCombatAsync(
                attackerBrigade.TokenId.Value,
                defenderBrigade.TokenId.Value,
                context);

            if (!combatResult.Success)
            {
                _logger.LogError("❌ WEAPON COMBAT CALCULATION FAILED: {ErrorMessage}", combatResult.ErrorMessage);
                throw new InvalidOperationException($"Weapon combat calculation failed: {combatResult.ErrorMessage}");
            }
            
            // Convert weapon casualties to percentages using REAL brigade strength
            var attackerStrength = CalculateBrigadeStrength(attackerBrigade);
            var defenderStrength = CalculateBrigadeStrength(defenderBrigade);

            // CRITICAL: Model SUSTAINED COMBAT over the entire engagement duration
            // A 3-hour meeting engagement isn't one exchange - it's sustained combat with maneuver and multiple engagements
            // Calculate engagement intensity based on duration
            var combatDurationMinutes = phase.DelayMinutes;
            var sustainedCombatMultiplier = Math.Max(1.0, combatDurationMinutes / 30.0); // Every 30 min = another engagement cycle
            
            // Apply multiplier to represent sustained combat
            var sustainedAttackerCasualties = combatResult.TotalDefenderCasualtiesInflicted * sustainedCombatMultiplier;
            var sustainedDefenderCasualties = combatResult.TotalAttackerCasualtiesInflicted * sustainedCombatMultiplier;

            // Calculate percentages from sustained combat
            var attackerCasualtyPct = (sustainedAttackerCasualties / attackerStrength) * 100.0;
            var defenderCasualtyPct = (sustainedDefenderCasualties / defenderStrength) * 100.0;

            // Apply doctrine-based bounds (meeting engagement / hasty defense)
            phase.CasualtiesAttacker = Math.Min(35, Math.Max(3, (int)attackerCasualtyPct)); // Meeting engagement: 3-35% attacker casualties
            phase.CasualtiesDefender = Math.Min(40, Math.Max(3, (int)defenderCasualtyPct)); // Hasty defense: 3-40% defender casualties

            _logger.LogInformation("✅ DIRECT ASSAULT ({Duration}min sustained): Single engagement: Att {AttRaw} / Def {DefRaw}. Sustained (x{Multiplier:F1}): Att {AttSustained:F0} / Def {DefSustained:F0}. Final: Att {AttPct:F2}% ({AttFinal}%) / Def {DefPct:F2}% ({DefFinal}%)", 
                combatDurationMinutes,
                combatResult.TotalDefenderCasualtiesInflicted, combatResult.TotalAttackerCasualtiesInflicted,
                sustainedCombatMultiplier,
                sustainedAttackerCasualties, sustainedDefenderCasualties,
                attackerCasualtyPct, phase.CasualtiesAttacker,
                defenderCasualtyPct, phase.CasualtiesDefender);

            // Calculate delay based on REAL force ratio from brigade composition  
            var attackerPower = CalculateBrigadeCombatPower(attackerBrigade);
            var defenderPower = CalculateBrigadeCombatPower(defenderBrigade);
            var ratio = attackerPower / defenderPower;
            
            if (ratio >= 2.0)
            {
                phase.DelayMinutes = 90; // 1.5 hours
            }
            else if (ratio >= 1.0)
            {
                phase.DelayMinutes = 150; // 2.5 hours
            }
            else
            {
                phase.DelayMinutes = 240; // 4 hours
            }

            // Add weapon engagement time and support timings
            phase.DelayMinutes += (combatResult.TotalEngagementTimeSeconds / 60);
            phase.DelayMinutes += 15; // Casualty evacuation
            phase.DelayMinutes += 10; // Ammunition resupply
            phase.DelayMinutes += 10; // Command coordination

            // Adjust for detection confidence
            if (detectionConfidence < 0.5)
            {
                phase.CasualtiesAttacker = (int)(phase.CasualtiesAttacker * 1.2);
                phase.DelayMinutes = (int)(phase.DelayMinutes * 1.2);
            }
            
            // Adjust for morale and fatigue
            // Brigade model doesn't store morale/fatigue - use standard combat-ready values
            // TODO: Add morale/fatigue tracking at Brigade or Token level for future enhancements
            var attackerMorale = 100;  // 100 = fully motivated, combat-ready troops
            var attackerFatigue = 0;   // 0 = fully rested troops
            
            if (attackerMorale < 50)
            {
                phase.CasualtiesAttacker = (int)(phase.CasualtiesAttacker * 1.1);
                phase.DelayMinutes = (int)(phase.DelayMinutes * 1.15);
            }
            
            if (attackerFatigue > 60)
            {
                phase.CasualtiesAttacker = (int)(phase.CasualtiesAttacker * 1.05);
                phase.DelayMinutes = (int)(phase.DelayMinutes * 1.1);
            }

            // Build detailed notes with weapon breakdown
            var weaponSummary = string.Join(", ", combatResult.AttackerWeaponResults
                .Where(w => w.TotalCasualties > 0.1)
                .Select(w => $"{w.WeaponType}({w.Quantity}): {w.TotalCasualties:F1} kills")
                .Take(3));

            phase.Notes = $"Force ratio: {ratio:F2}:1, Meeting engagement, Weapons: {weaponSummary}";

            return phase;
        }

        /// <summary>
        /// Simulate counter-attack by defender
        /// NOW USES WEAPON-LEVEL CALCULATIONS
        /// </summary>
        private async Task<DefensePhaseResult> SimulateCounterAttackAsync(
            Brigade? defenderBrigade,
            Brigade? attackerBrigade)
        {
            var phase = new DefensePhaseResult
            {
                PhaseName = "Counter-Attack",
                PhaseType = "Counter-Attack"
            };

            // Validate brigades - should never be null since we create temporary ones
            if (defenderBrigade == null || attackerBrigade == null)
            {
                _logger.LogError("❌ CRITICAL: Brigade is null - Defender: {DefenderNull}, Attacker: {AttackerNull}", 
                    defenderBrigade == null, 
                    attackerBrigade == null);
                throw new InvalidOperationException("Brigade data missing for counter-attack.");
            }
            
            // Validate TokenId is set
            if (!defenderBrigade.TokenId.HasValue || !attackerBrigade.TokenId.HasValue)
            {
                _logger.LogError("❌ CRITICAL: TokenId not set on brigade - DefenderTokenId: {DefenderTokenId}, AttackerTokenId: {AttackerTokenId}", 
                    defenderBrigade.TokenId, 
                    attackerBrigade.TokenId);
                throw new InvalidOperationException("Cannot calculate counter-attack without TokenId data");
            }

            // Get tokens for terrain
            var defenderToken = await _context.Tokens
                .Include(t => t.MapMarkers.Where(m => m.IsActive))
                .FirstOrDefaultAsync(t => t.Id == defenderBrigade.TokenId.Value);
            
            var attackerToken = await _context.Tokens
                .Include(t => t.MapMarkers.Where(m => m.IsActive))
                .FirstOrDefaultAsync(t => t.Id == attackerBrigade.TokenId.Value);

            // Get REAL terrain
            var terrain = defenderToken != null 
                ? await GetTerrainAtTokenPositionAsync(defenderToken) 
                : "open";

            // Create combat context - defender is now attacking, attacker is weakened
            var context = new CombatContext
            {
                Terrain = terrain,
                Weather = "clear",
                Visibility = "day clear",
                DefenderProtection = "hasty", // Attacker has hasty positions
                AttackerPosture = "moving fast", // Counter-attack is aggressive
                DefenderPosture = "hasty fortified" // Attacker has hasty defense
            };

            // Calculate weapon-level combat (defender attacks, attacker defends)
            var combatResult = await _unitCombatCalculator.CalculateUnitVsUnitCombatAsync(
                defenderBrigade.TokenId.Value, // Defender is attacking
                attackerBrigade.TokenId.Value, // Attacker is defending
                context);

            if (!combatResult.Success)
            {
                _logger.LogError("❌ Counter-attack combat calculation failed: {ErrorMessage}", combatResult.ErrorMessage);
                throw new InvalidOperationException($"Counter-attack calculation failed: {combatResult.ErrorMessage}");
            }

            // Calculate REAL casualties using brigade strength
            var defenderStrength = CalculateBrigadeStrength(defenderBrigade);
            var attackerStrength = CalculateBrigadeStrength(attackerBrigade);

            // Defender (counter-attacker) takes casualties from attacker's defensive fire
            phase.CasualtiesDefender = Math.Min(15, (int)((combatResult.TotalDefenderCasualtiesInflicted / defenderStrength) * 100));
            
            // Attacker (now defending) takes casualties - attacker is weakened (70% strength)
            var weakenedAttackerStrength = (int)(attackerStrength * 0.7);
            phase.CasualtiesAttacker = Math.Min(25, (int)((combatResult.TotalAttackerCasualtiesInflicted / weakenedAttackerStrength) * 100));

            // Calculate delay based on force ratio
            var defenderPower = CalculateBrigadeCombatPower(defenderBrigade);
            var attackerPower = CalculateBrigadeCombatPower(attackerBrigade) * 0.7; // Weakened
            var ratio = defenderPower / attackerPower;

            if (ratio >= 1.5)
            {
                phase.CounterAttackDelayMinutes = 45; // 45 min - strong counter-attack
                phase.Notes = $"Strong counter-attack - force ratio {ratio:F2}:1. Weapon casualties: Def {combatResult.TotalDefenderCasualtiesInflicted}, Att {combatResult.TotalAttackerCasualtiesInflicted}";
            }
            else if (ratio >= 1.0)
            {
                phase.CounterAttackDelayMinutes = 60; // 1 hour - balanced
                phase.Notes = $"Balanced counter-attack - force ratio {ratio:F2}:1. Weapon casualties: Def {combatResult.TotalDefenderCasualtiesInflicted}, Att {combatResult.TotalAttackerCasualtiesInflicted}";
            }
            else
            {
                phase.CounterAttackDelayMinutes = 90; // 1.5 hours - limited capability
                phase.Notes = $"Limited counter-attack - force ratio {ratio:F2}:1. Weapon casualties: Def {combatResult.TotalDefenderCasualtiesInflicted}, Att {combatResult.TotalAttackerCasualtiesInflicted}";
            }

            _logger.LogInformation("✅ Counter-attack calculated: Def {DefCas}%, Att {AttCas}%, Delay {Delay}min", 
                phase.CasualtiesDefender, phase.CasualtiesAttacker, phase.CounterAttackDelayMinutes);

            return phase;
        }

        /// <summary>
        /// Generate attack summary (2 lines)
        /// </summary>
        private AttackSummary GenerateAttackSummary(List<AttackPhaseResult> phases)
        {
            var summary = new AttackSummary();

            var engagementPhases = phases.Where(p => p.PhaseType == "Engagement").ToList();
            var combatPhases = phases.Where(p => p.PhaseType == "Combat").ToList();

            // Line 1: Engagement kill zones summary
            summary.EngagementKillZoneSummary = $"Engagement Kill Zones ({engagementPhases.Count}): " +
                $"Total Delay: {engagementPhases.Sum(p => p.DelayMinutes)} min, " +
                $"Attacker Casualties: {engagementPhases.Sum(p => p.CasualtiesAttacker)}%, " +
                $"Defender Casualties: {engagementPhases.Sum(p => p.CasualtiesDefender)}%";

            // Line 2: Defense positions assault summary
            summary.DefensePositionsSummary = $"Defense Positions ({combatPhases.Count}): " +
                $"Total Delay: {combatPhases.Sum(p => p.DelayMinutes)} min, " +
                $"Attacker Casualties: {combatPhases.Sum(p => p.CasualtiesAttacker)}%, " +
                $"Defender Casualties: {combatPhases.Sum(p => p.CasualtiesDefender)}%";

            // Overall totals
            summary.TotalDelayMinutes = phases.Sum(p => p.DelayMinutes);
            summary.TotalAttackerCasualties = phases.Sum(p => p.CasualtiesAttacker);
            summary.TotalDefenderCasualties = phases.Sum(p => p.CasualtiesDefender);

            return summary;
        }

        /// <summary>
        /// Generate defense summary (3 lines)
        /// </summary>
        private DefenseSummary GenerateDefenseSummary(List<DefensePhaseResult> phases)
        {
            var summary = new DefenseSummary();

            var holdPhase = phases.FirstOrDefault(p => p.PhaseType == "Defense");
            var repositionPhase = phases.FirstOrDefault(p => p.PhaseType == "Movement");
            var counterAttackPhase = phases.FirstOrDefault(p => p.PhaseType == "Counter-Attack");

            // Line 1: Time to stay in position
            if (holdPhase != null)
            {
                summary.TimeToStaySummary = $"Hold Position: {holdPhase.TimeToStayMinutes} min - {holdPhase.Notes}";
            }

            // Line 2: Movement to counter-penetration
            if (repositionPhase != null)
            {
                summary.CounterPenetrationMovementSummary = $"Reposition: {repositionPhase.MovementDelayMinutes} min, " +
                    $"Casualties: {repositionPhase.CasualtiesDefender}% - {repositionPhase.Notes}";
            }

            // Line 3: Counter-attack
            if (counterAttackPhase != null)
            {
                summary.CounterAttackSummary = $"Counter-Attack: Delay {counterAttackPhase.CounterAttackDelayMinutes} min, " +
                    $"Defender Casualties: {counterAttackPhase.CasualtiesDefender}%, " +
                    $"Attacker Casualties: {counterAttackPhase.CasualtiesAttacker}% - {counterAttackPhase.Notes}";
            }

            // Overall totals
            summary.TotalTimeMinutes = (holdPhase?.TimeToStayMinutes ?? 0) +
                                      (repositionPhase?.MovementDelayMinutes ?? 0) +
                                      (counterAttackPhase?.CounterAttackDelayMinutes ?? 0);

            summary.TotalDefenderCasualties = phases.Sum(p => p.CasualtiesDefender);
            summary.TotalAttackerCasualties = phases.Sum(p => p.CasualtiesAttacker);

            return summary;
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

        /// <summary>
        /// Get movement delay multiplier based on terrain
        /// </summary>
        private double GetTerrainMovementMultiplier(string? terrain)
        {
            if (string.IsNullOrEmpty(terrain))
                return 1.0;

            return terrain.ToLower() switch
            {
                "open" => 1.0,          // Normal movement
                "plain" => 1.0,
                "grassland" => 1.1,     // Slightly slower
                "forest" => 1.4,        // 40% slower
                "dense forest" => 1.6,  // 60% slower
                "jungle" => 1.8,        // 80% slower
                "hills" => 1.3,         // 30% slower
                "mountains" => 1.7,     // 70% slower
                "desert" => 1.2,        // 20% slower
                "swamp" => 1.8,         // 80% slower
                "urban" => 1.5,         // 50% slower
                "city" => 1.6,          // 60% slower
                "river" => 1.4,         // 40% slower (fording)
                _ => 1.2                // Default: 20% slower for unknown terrain
            };
        }

        /// <summary>
        /// Get terrain type from token position by checking MapSectors
        /// </summary>
        private async Task<string> GetTerrainAtTokenPositionAsync(Token token)
        {
            try
            {
                var marker = token.MapMarkers?.FirstOrDefault(m => m.IsActive);
                if (marker == null)
                    return "open"; // Default if no position
                
                var lat = double.Parse(marker.latitude);
                var lng = double.Parse(marker.longitude);
                
                // Query MapSectors to find terrain at this position
                // Check if point is within any sector's geometry
                var sectors = await _context.MapSectors
                    .Where(s => s.IsActive && s.SectorType == "terrain")
                    .ToListAsync();
                
                foreach (var sector in sectors)
                {
                    // Check if Properties contain terrain data
                    if (!string.IsNullOrEmpty(sector.Properties))
                    {
                        try
                        {
                            var props = System.Text.Json.JsonDocument.Parse(sector.Properties);
                            if (props.RootElement.TryGetProperty("terrain", out var terrainProp))
                            {
                                // Simple check: if sector has center coordinates, check distance
                                if (sector.CenterLat.HasValue && sector.CenterLng.HasValue)
                                {
                                    var distance = CalculateDistance(lat, lng, (double)sector.CenterLat.Value, (double)sector.CenterLng.Value);
                                    // If within ~5km of sector center, use that terrain
                                    if (distance < 5.0)
                                    {
                                        return terrainProp.GetString() ?? "open";
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Continue if JSON parsing fails
                        }
                    }
                }
                
                // Default if no terrain found
                return "open";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting terrain at token position, using default 'open'");
                return "open";
            }
        }

        /// <summary>
        /// Calculate brigade personnel strength from actual unit counts
        /// </summary>
        private int CalculateBrigadeStrength(Brigade? brigade)
        {
            if (brigade == null)
                return 500; // Fallback for null brigade
            
            int totalPersonnel = 0;
            
            // Infantry battalions: ~800 personnel each
            totalPersonnel += (brigade.InfantryBattalions?.Count ?? 0) * 800;
            
            // Armoured regiments: ~600 personnel each
            totalPersonnel += (brigade.ArmouredRegiments?.Count ?? 0) * 600;
            
            // Artillery regiments: ~400 personnel each
            totalPersonnel += (brigade.ArtilleryRegiments?.Count ?? 0) * 400;
            
            // Engineering companies: ~200 personnel each
            totalPersonnel += (brigade.CombatEngineeringCompanies?.Count ?? 0) * 200;
            
            // Logistics units: ~150 personnel each
            totalPersonnel += (brigade.LogisticsUnits?.Count ?? 0) * 150;
            
            return totalPersonnel > 0 ? totalPersonnel : 500; // Minimum 500 if no units
        }

        /// <summary>
        /// Calculate brigade combat power from weapon systems and unit composition
        /// </summary>
        private double CalculateBrigadeCombatPower(Brigade? brigade)
        {
            if (brigade == null)
                return 1.0; // Fallback
            
            double combatPower = 0;
            
            // Infantry battalions: base 100 combat power each
            combatPower += (brigade.InfantryBattalions?.Count ?? 0) * 100;
            
            // Armoured regiments: 150 combat power each (tanks are force multipliers)
            combatPower += (brigade.ArmouredRegiments?.Count ?? 0) * 150;
            
            // Artillery regiments: 80 combat power each (fire support)
            combatPower += (brigade.ArtilleryRegiments?.Count ?? 0) * 80;
            
            // Engineering companies: 30 combat power each (specialized support)
            combatPower += (brigade.CombatEngineeringCompanies?.Count ?? 0) * 30;
            
            return combatPower > 0 ? combatPower : 100.0; // Minimum 100 if no units
        }

        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }

    // ========================================
    // Result Models
    // ========================================

    public class ComprehensiveSimulationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SimulationTime { get; set; }
        
        // Token Information
        public string AttackerTokenName { get; set; } = string.Empty;
        public string DefenderTokenName { get; set; } = string.Empty;
        public bool WasSuspectedToken { get; set; }
        public double DetectionConfidence { get; set; }

        // Attack Phases
        public List<AttackPhaseResult> AttackPhases { get; set; } = new();
        
        // Defense Phases
        public List<DefensePhaseResult> DefensePhases { get; set; } = new();

        // Summaries
        public AttackSummary AttackSummary { get; set; } = new();
        public DefenseSummary DefenseSummary { get; set; } = new();

        // Victory Condition Tracking
        public string VictoryOutcome { get; set; } = string.Empty; // "Attacker Victory", "Defender Victory", "Stalemate"
        public string VictoryReason { get; set; } = string.Empty;
        public int TotalCombatRounds { get; set; }
        public int TotalEngagementTimeMinutes { get; set; }
        public int FinalAttackerCasualtiesPercent { get; set; }
        public int FinalDefenderCasualtiesPercent { get; set; }
    }

    public class AttackPhaseResult
    {
        public string PhaseName { get; set; } = string.Empty;
        public string PhaseType { get; set; } = string.Empty; // Movement, Engagement, Combat
        public string? Location { get; set; }
        public int DelayMinutes { get; set; }
        public int CasualtiesAttacker { get; set; } // Percentage
        public int CasualtiesDefender { get; set; } // Percentage
        public string Notes { get; set; } = string.Empty;
    }

    public class DefensePhaseResult
    {
        public string PhaseName { get; set; } = string.Empty;
        public string PhaseType { get; set; } = string.Empty; // Defense, Movement, Counter-Attack
        public int TimeToStayMinutes { get; set; }
        public int MovementDelayMinutes { get; set; }
        public int CounterAttackDelayMinutes { get; set; }
        public int CasualtiesDefender { get; set; } // Percentage
        public int CasualtiesAttacker { get; set; } // Percentage
        public string Notes { get; set; } = string.Empty;
    }

    public class AttackSummary
    {
        // Line 1: Engagement kill zones
        public string EngagementKillZoneSummary { get; set; } = string.Empty;
        
        // Line 2: Defense positions
        public string DefensePositionsSummary { get; set; } = string.Empty;

        // Totals
        public int TotalDelayMinutes { get; set; }
        public int TotalAttackerCasualties { get; set; }
        public int TotalDefenderCasualties { get; set; }
    }

    public class DefenseSummary
    {
        // Line 1: Time to stay
        public string TimeToStaySummary { get; set; } = string.Empty;
        
        // Line 2: Counter-penetration movement
        public string CounterPenetrationMovementSummary { get; set; } = string.Empty;
        
        // Line 3: Counter-attack
        public string CounterAttackSummary { get; set; } = string.Empty;

        // Totals
        public int TotalTimeMinutes { get; set; }
        public int TotalDefenderCasualties { get; set; }
        public int TotalAttackerCasualties { get; set; }
    }
}

