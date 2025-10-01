using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Models;

namespace TechWebSol.Services
{
    public interface IAttackPreviewService
    {
        Task<AttackPreviewResult> PreviewTokenAttackAsync(string attackerTokenId, string targetTokenId, int startTurn, string[]? artilleryAttached);
    }

    public class AttackPreviewService : IAttackPreviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDetectionService _detectionService;
        private readonly CombatService _combatService;
        private readonly IMovementService _movementService;
        private readonly ILogger<AttackPreviewService> _logger;

        public AttackPreviewService(
            ApplicationDbContext context,
            IDetectionService detectionService,
            CombatService combatService,
            IMovementService movementService,
            ILogger<AttackPreviewService> logger)
        {
            _context = context;
            _detectionService = detectionService;
            _combatService = combatService;
            _movementService = movementService;
            _logger = logger;
        }

        /// <summary>
        /// Preview attack outcome without making any changes to the database
        /// </summary>
        public async Task<AttackPreviewResult> PreviewTokenAttackAsync(string attackerTokenId, string targetTokenId, int startTurn, string[]? artilleryAttached)
        {
            try
            {
                // Parse string IDs to Guid
                if (!Guid.TryParse(attackerTokenId, out var attackerGuid) || !Guid.TryParse(targetTokenId, out var targetGuid))
                {
                    return new AttackPreviewResult
                    {
                        CanTarget = false,
                        UncertaintyNotes = "Invalid token ID format"
                    };
                }

                // Get attacker and target tokens
                var attackerToken = await _context.Tokens
                    .Include(t => t.MapMarkers.Where(m => m.IsActive))
                    .FirstOrDefaultAsync(t => t.Id == attackerGuid);

                var targetToken = await _context.Tokens
                    .Include(t => t.MapMarkers.Where(m => m.IsActive))
                    .FirstOrDefaultAsync(t => t.Id == targetGuid);

                if (attackerToken == null || targetToken == null)
                {
                    return new AttackPreviewResult
                    {
                        CanTarget = false,
                        UncertaintyNotes = "Attacker or target token not found"
                    };
                }

                // Get detection confidence
                var detectionConfidence = await _detectionService.GetDetectionConfidenceAsync(attackerTokenId, targetTokenId);
                var canTarget = await _detectionService.CanTargetAsync(attackerTokenId, targetTokenId, 0.5);

                // Get attacker's unit deployment for combat calculations
                var attackerDeployment = await _context.UnitDeployments
                    .FirstOrDefaultAsync(d => d.TokenId.ToString() == attackerTokenId);

                if (attackerDeployment == null)
                {
                    return new AttackPreviewResult
                    {
                        CanTarget = false,
                        UncertaintyNotes = "Attacker unit deployment not found"
                    };
                }

                // Calculate attacker's effective combat power
                var attackerCombatPower = CalculateEffectiveCombatPower(attackerDeployment, artilleryAttached);

                // Estimate defender's combat power (with uncertainty if low detection confidence)
                var defenderCombatPower = await EstimateDefenderCombatPowerAsync(targetToken, detectionConfidence);

                // Calculate movement needed
                var movementNeeded = await CalculateMovementNeededAsync(attackerToken, targetToken);
                var mpShortfall = Math.Max(0, movementNeeded - attackerDeployment.RemainingMovementPoints);

                // Calculate casualty estimates
                var casualtyEstimates = CalculateCasualtyEstimates(attackerCombatPower, defenderCombatPower, detectionConfidence);

                // Calculate probability of success
                var probabilityOfSuccess = CalculateProbabilityOfSuccess(attackerCombatPower, defenderCombatPower);

                // Generate supply warnings
                var supplyWarnings = GenerateSupplyWarnings(attackerDeployment);

                // Generate uncertainty notes
                var uncertaintyNotes = GenerateUncertaintyNotes(detectionConfidence, canTarget);

                return new AttackPreviewResult
                {
                    DetectionConfidence = detectionConfidence,
                    AttackerEffectiveCombatPower = attackerCombatPower,
                    DefenderEffectiveCombatPowerEstimated = defenderCombatPower,
                    AttackerExpectedCasualtyPercent = casualtyEstimates.AttackerCasualties,
                    DefenderExpectedCasualtyPercent = casualtyEstimates.DefenderCasualties,
                    ProbabilityOfSuccess = probabilityOfSuccess,
                    MovementNeededKm = movementNeeded,
                    MpShortfall = mpShortfall,
                    SupplyWarnings = supplyWarnings,
                    UncertaintyNotes = uncertaintyNotes,
                    CanTarget = canTarget,
                    AttackerTokenName = attackerToken.Name,
                    TargetTokenName = targetToken.Name
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing attack from {AttackerId} to {TargetId}", attackerTokenId, targetTokenId);
                return new AttackPreviewResult
                {
                    CanTarget = false,
                    UncertaintyNotes = "Error calculating attack preview"
                };
            }
        }

        /// <summary>
        /// Calculate effective combat power including artillery boost
        /// </summary>
        private double CalculateEffectiveCombatPower(UnitDeployment deployment, string[]? artilleryAttached)
        {
            var basePower = deployment.GetEffectiveCombatPower();

            // Apply artillery boost
            double artilleryBoost = 0.0;
            if (artilleryAttached != null && artilleryAttached.Any())
            {
                // For now, apply a flat 10% boost for any artillery support
                // In a full implementation, this would check artillery range, type, etc.
                artilleryBoost = 0.10;
            }

            return basePower * (1.0 + artilleryBoost);
        }

        /// <summary>
        /// Estimate defender's combat power with uncertainty based on detection confidence
        /// </summary>
        private async Task<double> EstimateDefenderCombatPowerAsync(Token targetToken, double detectionConfidence)
        {
            // Get target's unit deployment
            var targetDeployment = await _context.UnitDeployments
                .FirstOrDefaultAsync(d => d.TokenId == targetToken.Id);

            if (targetDeployment == null)
            {
                // Estimate based on token type if no deployment found
                return EstimateCombatPowerFromToken(targetToken);
            }

            var basePower = targetDeployment.GetEffectiveCombatPower();

            // Apply uncertainty based on detection confidence
            if (detectionConfidence < 0.5)
            {
                // Low confidence: inflate defender power by 10-30%
                var uncertaintyFactor = 1.0 + (0.3 - detectionConfidence * 0.4); // 1.1 to 1.3
                basePower *= uncertaintyFactor;
            }

            return basePower;
        }

        /// <summary>
        /// Estimate combat power from token when no deployment data is available
        /// </summary>
        private double EstimateCombatPowerFromToken(Token token)
        {
            // Basic estimation based on token name and force type
            // This is a simplified fallback
            return 1.0; // Default combat power
        }

        /// <summary>
        /// Calculate movement needed to reach target
        /// </summary>
        private async Task<double> CalculateMovementNeededAsync(Token attacker, Token target)
        {
            var attackerMarker = attacker.MapMarkers?.FirstOrDefault(m => m.IsActive);
            var targetMarker = target.MapMarkers?.FirstOrDefault(m => m.IsActive);

            if (attackerMarker == null || targetMarker == null)
                return 0.0;

            var attackerLat = double.Parse(attackerMarker.latitude);
            var attackerLng = double.Parse(attackerMarker.longitude);
            var targetLat = double.Parse(targetMarker.latitude);
            var targetLng = double.Parse(targetMarker.longitude);

            // Calculate distance in kilometers
            const double R = 6371; // Earth's radius in kilometers
            var dLat = ToRadians(targetLat - attackerLat);
            var dLng = ToRadians(targetLng - attackerLng);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(attackerLat)) * Math.Cos(ToRadians(targetLat)) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = R * c;

            return distance;
        }

        /// <summary>
        /// Calculate casualty estimates based on combat power ratio
        /// </summary>
        private (double AttackerCasualties, double DefenderCasualties) CalculateCasualtyEstimates(
            double attackerPower, double defenderPower, double detectionConfidence)
        {
            var ratio = attackerPower / defenderPower;

            // Base casualty estimates
            double attackerCasualties, defenderCasualties;

            if (ratio >= 3.0)
            {
                attackerCasualties = 0.065; // 3-10% average
                defenderCasualties = 0.25;  // 15-35% average
            }
            else if (ratio >= 1.5)
            {
                attackerCasualties = 0.09;  // 6-12% average
                defenderCasualties = 0.14;  // 8-20% average
            }
            else if (ratio >= 0.67)
            {
                attackerCasualties = 0.085; // 5-12% average
                defenderCasualties = 0.085; // 5-12% average
            }
            else
            {
                attackerCasualties = 0.185; // 12-25% average
                defenderCasualties = 0.05;  // 2-8% average
            }

            // Apply uncertainty if detection confidence is low
            if (detectionConfidence < 0.5)
            {
                var uncertaintyFactor = 1.0 + (0.5 - detectionConfidence);
                attackerCasualties *= uncertaintyFactor;
                defenderCasualties *= (2.0 - uncertaintyFactor); // Defender gets advantage
            }

            return (attackerCasualties * 100, defenderCasualties * 100); // Convert to percentage
        }

        /// <summary>
        /// Calculate probability of success based on combat power ratio
        /// </summary>
        private double CalculateProbabilityOfSuccess(double attackerPower, double defenderPower)
        {
            var ratio = attackerPower / defenderPower;

            return ratio switch
            {
                >= 3.0 => 0.85,
                >= 2.0 => 0.75,
                >= 1.5 => 0.65,
                >= 1.0 => 0.55,
                >= 0.67 => 0.45,
                _ => 0.25
            };
        }

        /// <summary>
        /// Generate supply warnings based on attacker's supply state
        /// </summary>
        private List<string> GenerateSupplyWarnings(UnitDeployment deployment)
        {
            var warnings = new List<string>();

            if (deployment.SupplyState == "Red")
            {
                warnings.Add("Critical supply state - combat effectiveness reduced by 40%");
            }
            else if (deployment.SupplyState == "Amber")
            {
                warnings.Add("Reduced supply state - combat effectiveness reduced by 15%");
            }

            if (deployment.Fatigue > 70)
            {
                warnings.Add("High fatigue level - movement and combat effectiveness reduced");
            }

            if (deployment.Morale < 50)
            {
                warnings.Add("Low morale - combat effectiveness significantly reduced");
            }

            return warnings;
        }

        /// <summary>
        /// Generate uncertainty notes based on detection confidence
        /// </summary>
        private string GenerateUncertaintyNotes(double detectionConfidence, bool canTarget)
        {
            if (!canTarget)
            {
                return "Target cannot be reliably detected. Attack planning not recommended.";
            }

            if (detectionConfidence < 0.3)
            {
                return "Very low detection confidence. Defender strength estimates may be significantly inaccurate.";
            }
            else if (detectionConfidence < 0.5)
            {
                return "Low detection confidence. Casualty estimates have high uncertainty.";
            }
            else if (detectionConfidence < 0.7)
            {
                return "Moderate detection confidence. Some uncertainty in estimates.";
            }

            return string.Empty;
        }

        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}
