using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Models;
using System.Text.Json;

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
        private readonly ILogger<ComprehensiveCombatSimulationService> _logger;

        public ComprehensiveCombatSimulationService(
            ApplicationDbContext context,
            IDetectionService detectionService,
            CombatService combatService,
            IMovementService movementService,
            ILogger<ComprehensiveCombatSimulationService> logger)
        {
            _context = context;
            _detectionService = detectionService;
            _combatService = combatService;
            _movementService = movementService;
            _logger = logger;
        }

        /// <summary>
        /// Comprehensive attack and defense simulation
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
                var (realDefenderToken, suspectedToken, detectionConfidence) = await ResolveSuspectedTokenAsync(targetTokenId);

                if (realDefenderToken == null)
                {
                    result.Success = false;
                    result.Message = "Target token not found or could not be resolved";
                    return result;
                }

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

                // Get deployments
                var attackerDeployment = await _context.UnitDeployments
                    .FirstOrDefaultAsync(d => d.TokenId == attackerTokenId);

                var defenderDeployment = await _context.UnitDeployments
                    .FirstOrDefaultAsync(d => d.TokenId == realDefenderToken.Id);

                // Store token information
                result.AttackerTokenName = attackerToken.Name;
                result.DefenderTokenName = realDefenderToken.Name;
                result.DetectionConfidence = detectionConfidence;
                result.WasSuspectedToken = suspectedToken != null;

                // ========================================
                // STEP 2: ATTACK SIMULATION
                // ========================================
                result.AttackPhases = await SimulateAttackPhasesAsync(
                    attackerToken,
                    realDefenderToken,
                    attackerDeployment,
                    defenderDeployment,
                    detectionConfidence);

                // ========================================
                // STEP 3: DEFENSE SIMULATION
                // ========================================
                result.DefensePhases = await SimulateDefensePhasesAsync(
                    realDefenderToken,
                    attackerToken,
                    defenderDeployment,
                    attackerDeployment);

                // ========================================
                // STEP 4: Generate Summary
                // ========================================
                result.AttackSummary = GenerateAttackSummary(result.AttackPhases);
                result.DefenseSummary = GenerateDefenseSummary(result.DefensePhases);

                result.Success = true;
                result.Message = "Simulation completed successfully";

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
        /// Resolve suspected token to real token
        /// </summary>
        private async Task<(Token? RealToken, SuspectedToken? SuspectedToken, double DetectionConfidence)> ResolveSuspectedTokenAsync(Guid targetTokenId)
        {
            // Check if target is a suspected token
            var suspectedToken = await _context.SuspectedTokens
                .Include(st => st.RealToken)
                    .ThenInclude(t => t.MapMarkers.Where(m => m.IsActive))
                .FirstOrDefaultAsync(st => st.Id == targetTokenId && st.RealTokenId != null);

            if (suspectedToken != null && suspectedToken.RealToken != null)
            {
                // Target is suspected token - resolve to real token
                var confidence = (double)(suspectedToken.MatchingConfidence ?? 50) / 100.0;
                _logger.LogInformation("Resolved suspected token {SuspectedId} to real token {RealId} with {Confidence}% confidence",
                    targetTokenId, suspectedToken.RealTokenId, suspectedToken.MatchingConfidence);
                
                return (suspectedToken.RealToken, suspectedToken, confidence);
            }

            // Target is already a real token
            var realToken = await _context.Tokens
                .Include(t => t.MapMarkers.Where(m => m.IsActive))
                .FirstOrDefaultAsync(t => t.Id == targetTokenId);

            return (realToken, null, 1.0); // 100% confidence for known token
        }

        /// <summary>
        /// Simulate attack phases through engagement zones and defense locations
        /// </summary>
        private async Task<List<AttackPhaseResult>> SimulateAttackPhasesAsync(
            Token attacker,
            Token defender,
            UnitDeployment? attackerDeployment,
            UnitDeployment? defenderDeployment,
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
            // PHASE 1: Movement to Contact (Approach)
            // ========================================
            var approachPhase = new AttackPhaseResult
            {
                PhaseName = "Approach Movement",
                PhaseType = "Movement"
            };

            var distanceKm = CalculateDistance(attackerLat, attackerLng, defenderLat, defenderLng);
            var movementSpeed = attackerDeployment?.MovementPointsPerTurn ?? 30; // km/turn
            approachPhase.DelayMinutes = (int)((distanceKm / movementSpeed) * 30); // 30 min per turn
            approachPhase.CasualtiesAttacker = 0; // No casualties during approach
            approachPhase.CasualtiesDefender = 0;
            approachPhase.Notes = $"Movement distance: {distanceKm:F2} km at {movementSpeed} km/turn";

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
                        attackerDeployment,
                        defenderDeployment,
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
                        attackerDeployment,
                        defenderDeployment,
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
                        attackerDeployment,
                        defenderDeployment,
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
                        attackerDeployment,
                        defenderDeployment,
                        position,
                        detectionConfidence);
                    
                    phases.Add(positionPhase);
                }
            }
            else
            {
                // No prepared positions - direct assault
                var directAssaultPhase = await SimulateDirectAssaultAsync(
                    attackerDeployment,
                    defenderDeployment,
                    detectionConfidence);
                
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
            UnitDeployment? defenderDeployment,
            UnitDeployment? attackerDeployment)
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

            var defenseStrength = defenderDeployment?.GetEffectiveCombatPower() ?? 1.0;
            var attackStrength = attackerDeployment?.GetEffectiveCombatPower() ?? 1.0;
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
            var defenseSpeed = defenderDeployment?.MovementPointsPerTurn ?? 25;
            repositionPhase.MovementDelayMinutes = (int)((repositionDistance / defenseSpeed) * 30);
            repositionPhase.CasualtiesDefender = (int)(defenseStrength * 0.02); // 2% casualties during movement
            repositionPhase.CasualtiesAttacker = 0;
            repositionPhase.Notes = $"Tactical repositioning: {repositionDistance:F1} km";

            phases.Add(repositionPhase);

            // ========================================
            // PHASE 3: Counter-Attack
            // ========================================
            var counterAttackPhase = await SimulateCounterAttackAsync(
                defenderDeployment,
                attackerDeployment);

            phases.Add(counterAttackPhase);

            return phases;
        }

        /// <summary>
        /// Simulate engagement through a kill zone
        /// </summary>
        private async Task<AttackPhaseResult> SimulateKillZoneEngagementAsync(
            UnitDeployment? attacker,
            UnitDeployment? defender,
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
            var attackerMorale = attacker?.Morale ?? 100;
            var attackerFatigue = attacker?.Fatigue ?? 0;
            
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
            var baseDelay = 15 + (killZoneEffectiveness * 30); // 15-45 minutes
            
            // Fatigue increases delay
            if (attackerFatigue > 60)
            {
                baseDelay *= 1.2; // 20% slower when fatigued
            }

            phase.DelayMinutes = (int)baseDelay;

            phase.Notes = $"Effectiveness: {killZoneEffectiveness:F2}, Detection: {detectionConfidence:P0}, Morale: {attackerMorale}, Fatigue: {attackerFatigue}";

            return phase;
        }

        /// <summary>
        /// Simulate engagement through a minefield
        /// </summary>
        private async Task<AttackPhaseResult> SimulateMinefieldEngagementAsync(
            UnitDeployment? attacker,
            UnitDeployment? defender,
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
            var attackerMorale = attacker?.Morale ?? 100;
            var attackerFatigue = attacker?.Fatigue ?? 0;
            
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
            if (attacker != null && attacker.UnitType.ToLower().Contains("engineer"))
            {
                baseAttackerCasualties *= 0.5; // Engineers reduce casualties by 50%
                phase.Notes += "Engineer support reduced minefield casualties. ";
            }

            phase.CasualtiesAttacker = (int)(baseAttackerCasualties);
            phase.CasualtiesDefender = (int)(baseDefenderCasualties);
            
            // Delay calculation (time to breach/clear minefield)
            var baseDelay = 30 + (minefieldEffectiveness * 60); // 30-90 minutes
            
            // Engineers speed up breaching
            if (attacker != null && attacker.UnitType.ToLower().Contains("engineer"))
            {
                baseDelay *= 0.6; // Engineers reduce time by 40%
            }
            
            // Fatigue increases delay
            if (attackerFatigue > 60)
            {
                baseDelay *= 1.3; // 30% slower when fatigued
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
            UnitDeployment? attacker,
            UnitDeployment? defender,
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
            var attackerMorale = attacker?.Morale ?? 100;
            var attackerFatigue = attacker?.Fatigue ?? 0;
            
            // Obstacles primarily cause delay, minimal casualties
            var baseAttackerCasualties = 2.0 + (obstacleEffectiveness * 5.0); // 2-7% casualties
            var baseDefenderCasualties = 1.0; // Minimal defender casualties

            // Adjust for morale (low morale = higher casualties while overcoming obstacles)
            if (attackerMorale < 50)
            {
                baseAttackerCasualties *= 1.4; // +40% casualties with low morale
            }

            // Engineers reduce casualties
            if (attacker != null && attacker.UnitType.ToLower().Contains("engineer"))
            {
                baseAttackerCasualties *= 0.3; // Engineers significantly reduce casualties
                phase.Notes = "Engineer support minimized obstacle casualties. ";
            }

            phase.CasualtiesAttacker = (int)(baseAttackerCasualties);
            phase.CasualtiesDefender = (int)(baseDefenderCasualties);
            
            // Delay calculation (time to breach/bypass obstacle)
            var baseDelay = 20 + (obstacleEffectiveness * 40); // 20-60 minutes
            
            // Engineers speed up breaching
            if (attacker != null && attacker.UnitType.ToLower().Contains("engineer"))
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
        /// </summary>
        private async Task<AttackPhaseResult> SimulateDefensePositionAssaultAsync(
            UnitDeployment? attacker,
            UnitDeployment? defender,
            DefenseElement position,
            double detectionConfidence)
        {
            var phase = new AttackPhaseResult
            {
                PhaseName = $"Assault: {position.Type} Position",
                PhaseType = "Combat",
                Location = position.Coordinates
            };

            var attackerPower = attacker?.GetEffectiveCombatPower() ?? 1.0;
            var defenderPower = (defender?.GetEffectiveCombatPower() ?? 1.0) * position.Effectiveness;

            var ratio = attackerPower / defenderPower;

            // Calculate casualties based on force ratio and prepared positions
            if (ratio >= 3.0)
            {
                phase.CasualtiesAttacker = 8;  // 8%
                phase.CasualtiesDefender = 30; // 30%
                phase.DelayMinutes = 45;
            }
            else if (ratio >= 1.5)
            {
                phase.CasualtiesAttacker = 15; // 15%
                phase.CasualtiesDefender = 20; // 20%
                phase.DelayMinutes = 90;
            }
            else if (ratio >= 0.8)
            {
                phase.CasualtiesAttacker = 20; // 20%
                phase.CasualtiesDefender = 15; // 15%
                phase.DelayMinutes = 120;
            }
            else
            {
                phase.CasualtiesAttacker = 30; // 30%
                phase.CasualtiesDefender = 10; // 10%
                phase.DelayMinutes = 180;
            }

            // Adjust for detection confidence
            if (detectionConfidence < 0.5)
            {
                phase.CasualtiesAttacker = (int)(phase.CasualtiesAttacker * 1.3);
                phase.DelayMinutes = (int)(phase.DelayMinutes * 1.2);
            }

            phase.Notes = $"Force ratio: {ratio:F2}:1, Position effectiveness: {position.Effectiveness:F2}";

            return phase;
        }

        /// <summary>
        /// Simulate direct assault (no prepared positions)
        /// </summary>
        private async Task<AttackPhaseResult> SimulateDirectAssaultAsync(
            UnitDeployment? attacker,
            UnitDeployment? defender,
            double detectionConfidence)
        {
            var phase = new AttackPhaseResult
            {
                PhaseName = "Direct Assault",
                PhaseType = "Combat"
            };

            var attackerPower = attacker?.GetEffectiveCombatPower() ?? 1.0;
            var defenderPower = defender?.GetEffectiveCombatPower() ?? 1.0;

            var ratio = attackerPower / defenderPower;

            // More favorable for attacker (no prepared positions)
            if (ratio >= 2.0)
            {
                phase.CasualtiesAttacker = 5;  // 5%
                phase.CasualtiesDefender = 25; // 25%
                phase.DelayMinutes = 30;
            }
            else if (ratio >= 1.0)
            {
                phase.CasualtiesAttacker = 10; // 10%
                phase.CasualtiesDefender = 15; // 15%
                phase.DelayMinutes = 60;
            }
            else
            {
                phase.CasualtiesAttacker = 18; // 18%
                phase.CasualtiesDefender = 8;  // 8%
                phase.DelayMinutes = 90;
            }

            // Adjust for poor intelligence
            if (detectionConfidence < 0.5)
            {
                phase.CasualtiesAttacker = (int)(phase.CasualtiesAttacker * 1.2);
            }

            phase.Notes = $"Force ratio: {ratio:F2}:1, Meeting engagement";

            return phase;
        }

        /// <summary>
        /// Simulate counter-attack by defender
        /// </summary>
        private async Task<DefensePhaseResult> SimulateCounterAttackAsync(
            UnitDeployment? defender,
            UnitDeployment? attacker)
        {
            var phase = new DefensePhaseResult
            {
                PhaseName = "Counter-Attack",
                PhaseType = "Counter-Attack"
            };

            var defenderPower = defender?.GetEffectiveCombatPower() ?? 1.0;
            var attackerPower = attacker?.GetEffectiveCombatPower() ?? 1.0;

            // Attacker is likely weakened from assault
            var ratio = defenderPower / (attackerPower * 0.7); // Attacker at 70% effectiveness

            if (ratio >= 1.5)
            {
                phase.CounterAttackDelayMinutes = 20;
                phase.CasualtiesDefender = 5;  // 5%
                phase.CasualtiesAttacker = 20; // 20%
                phase.Notes = "Strong counter-attack - attacker disrupted";
            }
            else if (ratio >= 1.0)
            {
                phase.CounterAttackDelayMinutes = 30;
                phase.CasualtiesDefender = 8;  // 8%
                phase.CasualtiesAttacker = 12; // 12%
                phase.Notes = "Balanced counter-attack";
            }
            else
            {
                phase.CounterAttackDelayMinutes = 45;
                phase.CasualtiesDefender = 12; // 12%
                phase.CasualtiesAttacker = 8;  // 8%
                phase.Notes = "Limited counter-attack capability";
            }

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

