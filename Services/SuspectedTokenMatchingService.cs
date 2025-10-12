using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using TechWebSol.Data;
using TechWebSol.Models;

namespace TechWebSol.Services
{
    public interface ISuspectedTokenMatchingService
    {
        Task<(Guid? RealTokenId, int? DistanceMeters, decimal? Confidence)> FindMatchingRealTokenAsync(
            decimal suspectedLat, decimal suspectedLng, string? suspectedName, string? suspectedType, string placerSide, Guid placerTeamId);
    }

    public class SuspectedTokenMatchingService : ISuspectedTokenMatchingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SuspectedTokenMatchingService> _logger;

        public SuspectedTokenMatchingService(
            ApplicationDbContext context,
            ILogger<SuspectedTokenMatchingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Find the matching real token based on UnitDesignation (primary) or multi-criteria (secondary)
        /// </summary>
        public async Task<(Guid? RealTokenId, int? DistanceMeters, decimal? Confidence)> FindMatchingRealTokenAsync(
            decimal suspectedLat, decimal suspectedLng, string? suspectedName, string? suspectedType, string placerSide, Guid placerTeamId)
        {
            try
            {
                // Get all tokens from OTHER teams (not the placer's team)
                // Suspected tokens are always about enemy tokens, so exclude same team
                var enemyTokens = await _context.Tokens
                    .Include(t => t.MapMarkers.Where(m => m.IsActive))
                    .Where(t => t.IsActive && 
                                t.TeamId != placerTeamId &&  // EXCLUDE same team
                                t.MapMarkers.Any(m => m.IsActive))
                    .ToListAsync();

                if (!enemyTokens.Any())
                {
                    _logger.LogWarning("No enemy tokens found (excluding team: {TeamId})", placerTeamId);
                    return (null, null, null);
                }

                // Extract number/designation from suspected token name (e.g., "38" from "Contact-38" or "38-Artillery")
                var suspectedDesignation = ExtractNumberFromName(suspectedName);

                // ========================================
                // PHASE 1: PRIMARY MATCH - UnitDesignation
                // ========================================
                if (!string.IsNullOrEmpty(suspectedDesignation))
                {
                    _logger.LogInformation("Phase 1: Looking for UnitDesignation match for: {Designation}", suspectedDesignation);

                    foreach (var token in enemyTokens)
                    {
                        // Check if UnitDesignation matches suspected designation
                        if (!string.IsNullOrEmpty(token.UnitDesignation) && 
                            token.UnitDesignation.Trim().Equals(suspectedDesignation, StringComparison.OrdinalIgnoreCase))
                        {
                            var activeMarker = token.MapMarkers.FirstOrDefault(m => m.IsActive);
                            if (activeMarker == null) continue;

                            // Calculate distance for logging
                            var tokenLat = decimal.Parse(activeMarker.latitude);
                            var tokenLng = decimal.Parse(activeMarker.longitude);
                            var distanceMeters = CalculateDistanceInMeters(
                                (double)suspectedLat, (double)suspectedLng,
                                (double)tokenLat, (double)tokenLng);

                            _logger.LogInformation(
                                "✅ PRIMARY MATCH FOUND: Token {TokenName} (UnitDesignation: {Designation}), Distance: {Distance}m",
                                token.Name, token.UnitDesignation, distanceMeters);

                            // Return with 100% confidence - perfect match
                            return (token.Id, distanceMeters, 100m);
                        }
                    }

                    _logger.LogInformation("No UnitDesignation match found for '{Designation}'. Proceeding to Phase 2...", suspectedDesignation);
                }

                // ========================================
                // PHASE 2: SECONDARY MATCH - Multi-Criteria
                // ========================================
                _logger.LogInformation("Phase 2: Using multi-criteria matching (type + proximity + name)");

                var matchCandidates = new List<TokenMatchCandidate>();

                foreach (var token in enemyTokens)
                {
                    var activeMarker = token.MapMarkers.FirstOrDefault(m => m.IsActive);
                    if (activeMarker == null) continue;

                    // Calculate distance
                    var tokenLat = decimal.Parse(activeMarker.latitude);
                    var tokenLng = decimal.Parse(activeMarker.longitude);
                    var distanceMeters = CalculateDistanceInMeters(
                        (double)suspectedLat, (double)suspectedLng,
                        (double)tokenLat, (double)tokenLng);

                    // Calculate matching scores
                    var typeMatchScore = CalculateTypeMatchScore(suspectedType, token.UnitType?.ToString());
                    var nameMatchScore = CalculateNameMatchScore(suspectedDesignation, ExtractNumberFromName(token.Name));
                    var proximityScore = CalculateProximityScore(distanceMeters);

                    // Calculate overall confidence
                    // Weights: Type=40%, Name=35%, Proximity=25%
                    var overallConfidence = (typeMatchScore * 0.40m) + 
                                           (nameMatchScore * 0.35m) + 
                                           (proximityScore * 0.25m);

                    matchCandidates.Add(new TokenMatchCandidate
                    {
                        TokenId = token.Id,
                        TokenName = token.Name,
                        UnitDesignation = token.UnitDesignation,
                        DistanceMeters = distanceMeters,
                        TypeMatchScore = typeMatchScore,
                        NameMatchScore = nameMatchScore,
                        ProximityScore = proximityScore,
                        OverallConfidence = overallConfidence
                    });
                }

                if (!matchCandidates.Any())
                {
                    _logger.LogWarning("No match candidates found");
                    return (null, null, null);
                }

                // Find best match
                var bestMatch = matchCandidates.OrderByDescending(c => c.OverallConfidence).First();

                // Only return match if confidence is above threshold (e.g., 30%)
                if (bestMatch.OverallConfidence < 30)
                {
                    _logger.LogInformation("No confident match found. Best match confidence: {Confidence}%", 
                        bestMatch.OverallConfidence);
                    return (null, null, null);
                }

                _logger.LogInformation(
                    "✅ SECONDARY MATCH FOUND: Token {TokenName} (Designation: {Designation}), Distance: {Distance}m, Confidence: {Confidence}%",
                    bestMatch.TokenName, bestMatch.UnitDesignation ?? "N/A", bestMatch.DistanceMeters, bestMatch.OverallConfidence);

                return (bestMatch.TokenId, bestMatch.DistanceMeters, bestMatch.OverallConfidence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding matching real token");
                return (null, null, null);
            }
        }

        /// <summary>
        /// Extract numbers from token name (e.g., "38" from "38-Artillery" or "Contact-38")
        /// </summary>
        private string? ExtractNumberFromName(string? name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            // Match any sequence of digits
            var match = Regex.Match(name, @"\d+");
            return match.Success ? match.Value : null;
        }

        /// <summary>
        /// Calculate type matching score (0-100)
        /// </summary>
        private decimal CalculateTypeMatchScore(string? suspectedType, string? realType)
        {
            if (string.IsNullOrEmpty(suspectedType) || string.IsNullOrEmpty(realType))
                return 50; // Neutral score if type unknown

            // Normalize types for comparison
            var suspected = suspectedType.ToLower().Trim();
            var real = realType.ToLower().Trim();

            // Exact match
            if (suspected == real) return 100;

            // Partial match (contains)
            if (suspected.Contains(real) || real.Contains(suspected)) return 70;

            // Category matching (e.g., "armored" matches "armor")
            if (suspected.StartsWith(real.Substring(0, Math.Min(4, real.Length))) ||
                real.StartsWith(suspected.Substring(0, Math.Min(4, suspected.Length))))
                return 60;

            return 20; // No match
        }

        /// <summary>
        /// Calculate name number matching score (0-100)
        /// </summary>
        private decimal CalculateNameMatchScore(string? suspectedNumber, string? realNumber)
        {
            if (string.IsNullOrEmpty(suspectedNumber) || string.IsNullOrEmpty(realNumber))
                return 30; // Low default if numbers not available

            // Exact number match
            if (suspectedNumber == realNumber) return 100;

            // Partial match (e.g., "90" matches "190")
            if (suspectedNumber.Contains(realNumber) || realNumber.Contains(suspectedNumber))
                return 50;

            return 0; // No match
        }

        /// <summary>
        /// Calculate proximity score based on distance (0-100)
        /// Closer tokens get higher scores
        /// </summary>
        private decimal CalculateProximityScore(int distanceMeters)
        {
            // Within 500m: 100%
            if (distanceMeters <= 500) return 100;

            // 500m-1km: 90-70%
            if (distanceMeters <= 1000) return 90 - ((distanceMeters - 500) * 0.04m);

            // 1km-5km: 70-30%
            if (distanceMeters <= 5000) return 70 - ((distanceMeters - 1000) * 0.01m);

            // 5km-10km: 30-10%
            if (distanceMeters <= 10000) return 30 - ((distanceMeters - 5000) * 0.004m);

            // Beyond 10km: 0-10%
            return Math.Max(0, 10 - ((distanceMeters - 10000) * 0.001m));
        }

        /// <summary>
        /// Calculate distance between two points in meters
        /// </summary>
        private int CalculateDistanceInMeters(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371000; // Earth's radius in meters
            var dLat = ToRadians(lat2 - lat1);
            var dLng = ToRadians(lng2 - lng1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return (int)(R * c);
        }

        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        private class TokenMatchCandidate
        {
            public Guid TokenId { get; set; }
            public string TokenName { get; set; } = string.Empty;
            public string? UnitDesignation { get; set; }
            public int DistanceMeters { get; set; }
            public decimal TypeMatchScore { get; set; }
            public decimal NameMatchScore { get; set; }
            public decimal ProximityScore { get; set; }
            public decimal OverallConfidence { get; set; }
        }
    }
}

