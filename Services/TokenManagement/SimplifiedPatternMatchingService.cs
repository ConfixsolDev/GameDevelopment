using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Models;
using System.Text.Json;

namespace TechWebSol.Services.TokenManagement
{
    /// <summary>
    /// Simplified pattern matching service implementation
    /// Only uses geometric properties: distances, angles, and center point
    /// </summary>
    public class SimplifiedPatternMatchingService : ISimplifiedPatternMatchingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SimplifiedPatternMatchingService> _logger;

        public SimplifiedPatternMatchingService(ApplicationDbContext context, ILogger<SimplifiedPatternMatchingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TokenIdentificationResult> IdentifyTokenAsync(GeometricData currentData, double confidenceThreshold = 70.0)
        {
            try
            {
                _logger.LogInformation("Starting simplified token identification for {TouchCount} points", 
                    currentData.Distances.Length);

                // Get all active tokens with their signatures
                var tokens = await _context.Tokens
                    .Include(t => t.Signature)
                    .Where(t => t.IsActive && t.Signature != null)
                    .ToListAsync();

                if (!tokens.Any())
                {
                    return new TokenIdentificationResult
                    {
                        Success = false,
                        Message = "No active tokens found for comparison"
                    };
                }

                var bestMatch = new TokenMatch
                {
                    Confidence = 0,
                    Token = null
                };

                var matchDetails = new List<TokenMatchDetail>();

                // Compare with each stored token
                foreach (var token in tokens)
                {
                    var storedData = ParseGeometricData(token.Signature!);
                    var similarityResult = CalculatePatternSimilarity(currentData, storedData);
                    
                    var matchDetail = new TokenMatchDetail
                    {
                        TokenId = token.Id,
                        TokenName = token.Name,
                        Confidence = similarityResult.OverallSimilarity,
                        DistanceSimilarity = similarityResult.DistanceSimilarity,
                        ShapeSimilarity = similarityResult.ShapeSimilarity,
                        TimingSimilarity = similarityResult.TimingSimilarity,
                        GeometricSimilarity = similarityResult.GeometricSimilarity,
                        MatchFactors = similarityResult.MatchFactors
                    };

                    matchDetails.Add(matchDetail);

                    if (similarityResult.OverallSimilarity > bestMatch.Confidence && 
                        similarityResult.OverallSimilarity >= confidenceThreshold)
                    {
                        bestMatch = new TokenMatch
                        {
                            Confidence = similarityResult.OverallSimilarity,
                            Token = token,
                            MatchDetails = matchDetail
                        };
                    }
                }

                // Update usage statistics if match found
                if (bestMatch.Token != null)
                {
                    bestMatch.Token.LastUsed = DateTime.UtcNow;
                    bestMatch.Token.UsageCount++;
                    await _context.SaveChangesAsync();
                }

                return new TokenIdentificationResult
                {
                    Success = bestMatch.Token != null,
                    MatchedToken = bestMatch.Token,
                    Confidence = bestMatch.Confidence,
                    AllMatches = matchDetails.OrderByDescending(m => m.Confidence).ToList(),
                    Message = bestMatch.Token != null 
                        ? $"Token '{bestMatch.Token.Name}' identified with {bestMatch.Confidence:F1}% confidence"
                        : $"No token found above {confidenceThreshold}% confidence threshold"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during simplified token identification");
                return new TokenIdentificationResult
                {
                    Success = false,
                    Message = "Error during token identification"
                };
            }
        }

        public async Task<PatternSimilarityResult> CalculatePatternSimilarityAsync(GeometricData data1, GeometricData data2)
        {
            return await Task.FromResult(CalculatePatternSimilarity(data1, data2));
        }

        public async Task<bool> ValidatePatternConsistencyAsync(List<GeometricData> trainingData)
        {
            try
            {
                if (trainingData.Count < 2)
                    return false;

                var firstData = trainingData[0];
                var consistencyScores = new List<double>();

                for (int i = 1; i < trainingData.Count; i++)
                {
                    var similarity = CalculatePatternSimilarity(firstData, trainingData[i]);
                    consistencyScores.Add(similarity.OverallSimilarity);
                }

                var averageConsistency = consistencyScores.Average();
                var minConsistency = consistencyScores.Min();

                // Require at least 80% average consistency and 70% minimum consistency
                return averageConsistency >= 80 && minConsistency >= 70;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating pattern consistency");
                return false;
            }
        }

        public async Task<GeometricData> CalculateGeometricDataAsync(double[][] touchPoints)
        {
            return await Task.FromResult(CalculateGeometricData(touchPoints));
        }

        /// <summary>
        /// Calculate geometric data from touch points
        /// </summary>
        private GeometricData CalculateGeometricData(double[][] touchPoints)
        {
            var data = new GeometricData();
            
            if (touchPoints.Length < 2)
                return data;

            // Calculate distances between all points
            data.Distances = CalculateDistances(touchPoints);
            
            // Calculate angles if we have 3+ points
            if (touchPoints.Length >= 3)
            {
                data.Angles = CalculateAngles(touchPoints);
            }
            
            // Calculate center point
            data.Center = CalculateCenter(touchPoints);
            
            return data;
        }

        /// <summary>
        /// Calculate distances between all touch points
        /// </summary>
        private double[] CalculateDistances(double[][] points)
        {
            var distances = new List<double>();
            
            for (int i = 0; i < points.Length; i++)
            {
                for (int j = i + 1; j < points.Length; j++)
                {
                    var dx = points[j][0] - points[i][0];
                    var dy = points[j][1] - points[i][1];
                    var distance = Math.Sqrt(dx * dx + dy * dy);
                    distances.Add(distance);
                }
            }
            
            return distances.ToArray();
        }

        /// <summary>
        /// Calculate angles formed by the shape
        /// </summary>
        private double[] CalculateAngles(double[][] points)
        {
            var angles = new List<double>();
            
            for (int i = 0; i < points.Length; i++)
            {
                var prevIndex = (i - 1 + points.Length) % points.Length;
                var nextIndex = (i + 1) % points.Length;
                
                var angle = CalculateAngle(
                    points[prevIndex], points[i], points[nextIndex]
                );
                angles.Add(angle);
            }
            
            return angles.ToArray();
        }

        /// <summary>
        /// Calculate angle at point B formed by points A-B-C
        /// </summary>
        private double CalculateAngle(double[] pointA, double[] pointB, double[] pointC)
        {
            // Vector BA
            var baX = pointA[0] - pointB[0];
            var baY = pointA[1] - pointB[1];
            
            // Vector BC
            var bcX = pointC[0] - pointB[0];
            var bcY = pointC[1] - pointB[1];
            
            // Calculate angle using dot product
            var dotProduct = baX * bcX + baY * bcY;
            var magnitudeBA = Math.Sqrt(baX * baX + baY * baY);
            var magnitudeBC = Math.Sqrt(bcX * bcX + bcY * bcY);
            
            if (magnitudeBA == 0 || magnitudeBC == 0)
                return 0;
            
            var cosAngle = dotProduct / (magnitudeBA * magnitudeBC);
            cosAngle = Math.Max(-1, Math.Min(1, cosAngle)); // Clamp to avoid NaN
            
            return Math.Acos(cosAngle) * 180 / Math.PI; // Convert to degrees
        }

        /// <summary>
        /// Calculate center point of all touch points
        /// </summary>
        private CenterPoint CalculateCenter(double[][] points)
        {
            var centerX = points.Average(p => p[0]);
            var centerY = points.Average(p => p[1]);
            
            return new CenterPoint { X = centerX, Y = centerY };
        }

        /// <summary>
        /// Calculate pattern similarity between two geometric data sets
        /// </summary>
        private PatternSimilarityResult CalculatePatternSimilarity(GeometricData data1, GeometricData data2)
        {
            try
            {
                // Check if touch counts match
                if (data1.Distances.Length != data2.Distances.Length)
                {
                return new PatternSimilarityResult
                {
                    OverallSimilarity = 0,
                    MatchFactors = new Dictionary<string, double> { { "Touch count mismatch", 0 } }
                };
                }

                // Calculate distance similarity (60% weight)
                var distanceSimilarity = CalculateDistanceSimilarity(data1.Distances, data2.Distances);

                // Calculate angle similarity (30% weight) - only if both have angles
                var angleSimilarity = 50.0; // Default neutral score
                if (data1.Angles.Length > 0 && data2.Angles.Length > 0)
                {
                    angleSimilarity = CalculateAngleSimilarity(data1.Angles, data2.Angles);
                }

                // Calculate center similarity (10% weight)
                var centerSimilarity = CalculateCenterSimilarity(data1.Center, data2.Center);

                // Calculate weighted overall similarity
                var overallSimilarity = (distanceSimilarity * 0.6) + 
                                      (angleSimilarity * 0.3) + 
                                      (centerSimilarity * 0.1);

                var matchFactors = new Dictionary<string, double>();
                if (distanceSimilarity > 90) matchFactors.Add("Excellent distance match", distanceSimilarity);
                if (angleSimilarity > 90) matchFactors.Add("Excellent angle match", angleSimilarity);
                if (centerSimilarity > 90) matchFactors.Add("Excellent center match", centerSimilarity);

                return new PatternSimilarityResult
                {
                    OverallSimilarity = Math.Round(overallSimilarity, 2),
                    DistanceSimilarity = Math.Round(distanceSimilarity, 2),
                    ShapeSimilarity = Math.Round(angleSimilarity, 2), // Map angle to shape
                    TimingSimilarity = 50.0, // Default neutral score for timing
                    GeometricSimilarity = Math.Round(centerSimilarity, 2), // Map center to geometric
                    MatchFactors = matchFactors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating pattern similarity");
                return new PatternSimilarityResult { OverallSimilarity = 0 };
            }
        }

        /// <summary>
        /// Calculate distance similarity
        /// </summary>
        private double CalculateDistanceSimilarity(double[] distances1, double[] distances2)
        {
            if (distances1.Length != distances2.Length)
                return 0;

            // Sort distances for consistent comparison
            var sorted1 = distances1.OrderBy(d => d).ToArray();
            var sorted2 = distances2.OrderBy(d => d).ToArray();

            // Calculate adaptive tolerance based on average distance
            var avgDistance = (sorted1.Average() + sorted2.Average()) / 2;
            var tolerance = Math.Max(5, avgDistance * 0.1); // 10% of average distance, minimum 5px

            double totalSimilarity = 0;
            int validComparisons = 0;

            for (int i = 0; i < sorted1.Length; i++)
            {
                var difference = Math.Abs(sorted1[i] - sorted2[i]);
                
                if (difference <= tolerance)
                {
                    // Within tolerance - high similarity
                    var similarity = Math.Max(0, 100 - (difference / tolerance * 20));
                    totalSimilarity += similarity;
                }
                else
                {
                    // Outside tolerance - lower similarity
                    var similarity = Math.Max(0, 100 - (difference * 2));
                    totalSimilarity += similarity;
                }
                
                validComparisons++;
            }

            return validComparisons > 0 ? totalSimilarity / validComparisons : 0;
        }

        /// <summary>
        /// Calculate angle similarity
        /// </summary>
        private double CalculateAngleSimilarity(double[] angles1, double[] angles2)
        {
            if (angles1.Length != angles2.Length)
                return 0;

            // Sort angles for consistent comparison
            var sorted1 = angles1.OrderBy(a => a).ToArray();
            var sorted2 = angles2.OrderBy(a => a).ToArray();

            double totalSimilarity = 0;
            int validComparisons = 0;

            for (int i = 0; i < sorted1.Length; i++)
            {
                var difference = Math.Abs(sorted1[i] - sorted2[i]);
                
                // Handle angle wrap-around (e.g., 350° and 10° are close)
                if (difference > 180)
                    difference = 360 - difference;
                
                // Angle tolerance: 15 degrees
                var tolerance = 15;
                
                if (difference <= tolerance)
                {
                    var similarity = Math.Max(0, 100 - (difference / tolerance * 20));
                    totalSimilarity += similarity;
                }
                else
                {
                    var similarity = Math.Max(0, 100 - (difference * 1.5));
                    totalSimilarity += similarity;
                }
                
                validComparisons++;
            }

            return validComparisons > 0 ? totalSimilarity / validComparisons : 0;
        }

        /// <summary>
        /// Calculate center point similarity
        /// </summary>
        private double CalculateCenterSimilarity(CenterPoint center1, CenterPoint center2)
        {
            var dx = center1.X - center2.X;
            var dy = center1.Y - center2.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            
            // Center tolerance: 20 pixels
            var tolerance = 20;
            
            if (distance <= tolerance)
            {
                return Math.Max(0, 100 - (distance / tolerance * 20));
            }
            else
            {
                return Math.Max(0, 100 - (distance * 2));
            }
        }

        /// <summary>
        /// Parse geometric data from token signature
        /// </summary>
        private GeometricData ParseGeometricData(TokenSignature signature)
        {
            var data = new GeometricData();
            
            try
            {
                if (!string.IsNullOrEmpty(signature.Distances))
                {
                    data.Distances = JsonSerializer.Deserialize<double[]>(signature.Distances) ?? Array.Empty<double>();
                }
                
                if (!string.IsNullOrEmpty(signature.Angles))
                {
                    data.Angles = JsonSerializer.Deserialize<double[]>(signature.Angles) ?? Array.Empty<double>();
                }
                
                if (!string.IsNullOrEmpty(signature.Center))
                {
                    data.Center = JsonSerializer.Deserialize<CenterPoint>(signature.Center) ?? new CenterPoint();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing geometric data from signature");
            }
            
            return data;
        }

        private class TokenMatch
        {
            public double Confidence { get; set; }
            public Token? Token { get; set; }
            public TokenMatchDetail? MatchDetails { get; set; }
        }
    }
}
