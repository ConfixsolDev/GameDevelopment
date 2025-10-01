using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Models;

namespace TechWebSol.Services
{
    public interface IDetectionService
    {
        Task<double> GetDetectionConfidenceAsync(string attackerTokenId, string targetTokenId);
        Task<bool> CanTargetAsync(string attackerTokenId, string targetTokenId, double minConfidence = 0.5);
        Task<double> CalculateTerrainDetectionModifierAsync(string terrainType);
        Task<double> CalculateReconProximityBonusAsync(string attackerTokenId, string targetTokenId);
    }

    public class DetectionService : IDetectionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DetectionService> _logger;

        public DetectionService(ApplicationDbContext context, ILogger<DetectionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Calculate detection confidence for targeting an enemy token
        /// Combines SuspectedToken confidence, terrain modifiers, recency, and recon proximity
        /// </summary>
        public async Task<double> GetDetectionConfidenceAsync(string attackerTokenId, string targetTokenId)
        {
            try
            {
                // Parse string IDs to Guid
                if (!Guid.TryParse(attackerTokenId, out var attackerGuid) || !Guid.TryParse(targetTokenId, out var targetGuid))
                {
                    _logger.LogWarning("Invalid token ID format: {AttackerId}, {TargetId}", attackerTokenId, targetTokenId);
                    return 0.0;
                }

                // Get attacker and target tokens
                var attackerToken = await _context.Tokens
                    .Include(t => t.MapMarkers)
                    .FirstOrDefaultAsync(t => t.Id == attackerGuid);

                var targetToken = await _context.Tokens
                    .Include(t => t.MapMarkers)
                    .FirstOrDefaultAsync(t => t.Id == targetGuid);

                if (attackerToken == null || targetToken == null)
                {
                    _logger.LogWarning("Attacker or target token not found: {AttackerId}, {TargetId}", attackerTokenId, targetTokenId);
                    return 0.0;
                }

                // Base confidence from SuspectedToken if target is suspected
                var targetMarker = targetToken.MapMarkers?.FirstOrDefault(m => m.IsActive);
                var suspectedToken = await _context.SuspectedTokens
                    .FirstOrDefaultAsync(st => 
                        targetMarker != null &&
                        Math.Abs((double)st.Latitude - double.Parse(targetMarker.latitude)) < 0.001 &&
                        Math.Abs((double)st.Longitude - double.Parse(targetMarker.longitude)) < 0.001);

                double baseConfidence = suspectedToken != null ? (double)suspectedToken.Confidence / 100.0 : 1.0;

                // If target is confirmed token (not suspected), use full confidence
                if (suspectedToken == null)
                {
                    baseConfidence = 1.0;
                }

                // Apply terrain detection modifier
                var terrainModifier = await CalculateTerrainDetectionModifierAsync("OPEN"); // Default terrain
                baseConfidence *= terrainModifier;

                // Apply recency modifier (how recent was the last contact)
                var recencyModifier = CalculateRecencyModifier(suspectedToken?.LastConfirmedAt);
                baseConfidence *= recencyModifier;

                // Apply recon proximity bonus
                var reconBonus = await CalculateReconProximityBonusAsync(attackerTokenId, targetTokenId);
                baseConfidence = Math.Min(1.0, baseConfidence + reconBonus);

                // Ensure confidence is within bounds
                return Math.Max(0.0, Math.Min(1.0, baseConfidence));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating detection confidence for {AttackerId} -> {TargetId}", attackerTokenId, targetTokenId);
                return 0.0;
            }
        }

        /// <summary>
        /// Check if attacker can target the enemy token based on detection confidence
        /// </summary>
        public async Task<bool> CanTargetAsync(string attackerTokenId, string targetTokenId, double minConfidence = 0.5)
        {
            var confidence = await GetDetectionConfidenceAsync(attackerTokenId, targetTokenId);
            return confidence >= minConfidence;
        }

        /// <summary>
        /// Calculate terrain-based detection modifier
        /// </summary>
        public async Task<double> CalculateTerrainDetectionModifierAsync(string terrainType)
        {
            return terrainType?.ToLower() switch
            {
                "open" => 1.0,
                "desert" => 0.9,
                "plain" => 0.95,
                "forest" => 0.6,
                "urban" => 0.4,
                "mountain" => 0.7,
                "swamp" => 0.5,
                _ => 1.0
            };
        }

        /// <summary>
        /// Calculate recon proximity bonus based on friendly recon units near target
        /// </summary>
        public async Task<double> CalculateReconProximityBonusAsync(string attackerTokenId, string targetTokenId)
        {
            try
            {
                // Get target position
                var targetToken = await _context.Tokens
                    .Include(t => t.MapMarkers.Where(m => m.IsActive))
                    .FirstOrDefaultAsync(t => t.Id.ToString() == targetTokenId);

                if (targetToken?.MapMarkers?.FirstOrDefault() == null)
                    return 0.0;

                var targetLat = double.Parse(targetToken.MapMarkers.First().latitude);
                var targetLng = double.Parse(targetToken.MapMarkers.First().longitude);

                // Find nearby friendly recon units (within 10km)
                var nearbyRecon = await _context.Recon
                    .Include(r => r.Token)
                    .ThenInclude(t => t.MapMarkers)
                    .Where(r => r.TeamId == targetToken.TeamId) // Same team as attacker
                    .ToListAsync();

                double maxBonus = 0.0;
                foreach (var recon in nearbyRecon)
                {
                    if (recon.Token?.MapMarkers?.FirstOrDefault(m => m.IsActive) != null)
                    {
                        var reconMarker = recon.Token.MapMarkers.First(m => m.IsActive);
                        var reconLat = double.Parse(reconMarker.latitude);
                        var reconLng = double.Parse(reconMarker.longitude);
                        
                        // Calculate distance (simplified)
                        var distance = CalculateDistance(targetLat, targetLng, reconLat, reconLng);
                        
                        if (distance <= 10.0) // Within 10km
                        {
                            var bonus = Math.Max(0.0, 0.2 * (1.0 - distance / 10.0)); // Up to 20% bonus
                            maxBonus = Math.Max(maxBonus, bonus);
                        }
                    }
                }

                return maxBonus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating recon proximity bonus");
                return 0.0;
            }
        }

        /// <summary>
        /// Calculate recency modifier based on last confirmed contact time
        /// </summary>
        private double CalculateRecencyModifier(DateTime? lastConfirmedAt)
        {
            if (lastConfirmedAt == null)
                return 0.8; // Unknown recency, reduce confidence

            var hoursSinceContact = (DateTime.UtcNow - lastConfirmedAt.Value).TotalHours;
            
            return hoursSinceContact switch
            {
                <= 1 => 1.0,    // Very recent
                <= 6 => 0.9,    // Recent
                <= 24 => 0.7,   // Yesterday
                <= 72 => 0.5,   // 3 days ago
                _ => 0.3        // Very old
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
    }
}
