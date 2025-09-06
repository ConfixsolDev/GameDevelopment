using System.Text.Json;
using TechWebSol.Models;
using TechWebSol.Services.TokenManagement;

namespace TechWebSol.Services.TokenManagement
{
    public class PatternAnalysisEngine
    {
        private readonly ILogger<PatternAnalysisEngine> _logger;

        public PatternAnalysisEngine(ILogger<PatternAnalysisEngine> logger)
        {
            _logger = logger;
        }

        public async Task<PatternSimilarityResult> CalculatePatternSimilarityAsync(TokenSignature signature1, TokenSignature signature2)
        {
            try
            {
                // 1. Basic validation
                if (signature1.TouchCount != signature2.TouchCount)
                {
                    return new PatternSimilarityResult
                    {
                        OverallSimilarity = 0,
                        MatchFactors = new List<string> { "Touch count mismatch" }
                    };
                }

                // 2. Distance-based similarity (40% weight)
                var distanceSimilarity = CalculateDistanceSimilarity(signature1, signature2);

                // 3. Shape-based similarity (30% weight)
                var shapeSimilarity = CalculateShapeSimilarity(signature1, signature2);

                // 4. Timing-based similarity (20% weight)
                var timingSimilarity = CalculateTimingSimilarity(signature1, signature2);

                // 5. Geometric similarity (10% weight)
                var geometricSimilarity = CalculateGeometricSimilarity(signature1, signature2);

                // 6. Calculate weighted overall similarity
                var overallSimilarity = (distanceSimilarity * 0.4) + 
                                      (shapeSimilarity * 0.3) + 
                                      (timingSimilarity * 0.2) + 
                                      (geometricSimilarity * 0.1);

                var matchFactors = new List<string>();
                if (distanceSimilarity > 90) matchFactors.Add("Excellent distance match");
                if (shapeSimilarity > 90) matchFactors.Add("Excellent shape match");
                if (timingSimilarity > 90) matchFactors.Add("Excellent timing match");
                if (geometricSimilarity > 90) matchFactors.Add("Excellent geometric match");

                return new PatternSimilarityResult
                {
                    OverallSimilarity = Math.Round(overallSimilarity, 2),
                    DistanceSimilarity = Math.Round(distanceSimilarity, 2),
                    ShapeSimilarity = Math.Round(shapeSimilarity, 2),
                    TimingSimilarity = Math.Round(timingSimilarity, 2),
                    GeometricSimilarity = Math.Round(geometricSimilarity, 2),
                    MatchFactors = matchFactors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating pattern similarity");
                return new PatternSimilarityResult { OverallSimilarity = 0 };
            }
        }

        private double CalculateDistanceSimilarity(TokenSignature signature1, TokenSignature signature2)
        {
            try
            {
                var pattern1 = signature1.TouchPattern;
                var pattern2 = signature2.TouchPattern;

                if (pattern1?.Distances == null || pattern2?.Distances == null)
                    return 0;

                var distances1 = JsonSerializer.Deserialize<double[]>(pattern1.Distances) ?? new double[0];
                var distances2 = JsonSerializer.Deserialize<double[]>(pattern2.Distances) ?? new double[0];

                if (distances1.Length != distances2.Length)
                    return 0;

                // Sort distances for consistent comparison
                Array.Sort(distances1);
                Array.Sort(distances2);

                // Calculate adaptive tolerance based on pattern complexity
                var baseTolerance = 4.0; // Base 4px tolerance
                var complexityFactor = Math.Min(signature1.TouchCount, 5);
                var adaptiveTolerance = baseTolerance + (complexityFactor - 2) * 0.5;

                double totalSimilarity = 0;
                int validComparisons = 0;

                for (int i = 0; i < distances1.Length; i++)
                {
                    if (distances1[i] > 0 && distances2[i] > 0)
                    {
                        var pixelDifference = Math.Abs(distances1[i] - distances2[i]);
                        
                        if (pixelDifference <= adaptiveTolerance)
                        {
                            // Within tolerance - high similarity
                            var similarity = Math.Max(0, 100 - (pixelDifference / adaptiveTolerance * 15));
                            totalSimilarity += similarity;
                        }
                        else
                        {
                            // Outside tolerance - lower similarity but not zero
                            var similarity = Math.Max(0, 100 - (pixelDifference * 1.5));
                            totalSimilarity += similarity;
                        }
                        
                        validComparisons++;
                    }
                }

                return validComparisons > 0 ? totalSimilarity / validComparisons : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating distance similarity");
                return 0;
            }
        }

        private double CalculateShapeSimilarity(TokenSignature signature1, TokenSignature signature2)
        {
            try
            {
                var geometry1 = signature1.MultiTouchGeometry;
                var geometry2 = signature2.MultiTouchGeometry;

                if (geometry1 == null || geometry2 == null)
                    return 50; // Neutral score if no geometry data

                var similarities = new List<double>();

                // Aspect ratio similarity
                if (geometry1.AspectRatio > 0 && geometry2.AspectRatio > 0)
                {
                    var aspectRatioDiff = Math.Abs((double)geometry1.AspectRatio - (double)geometry2.AspectRatio);
                    var aspectRatioSim = Math.Max(0, 100 - (aspectRatioDiff * 50));
                    similarities.Add(aspectRatioSim);
                }

                // Bounding box area similarity
                if (geometry1.BoundingBoxArea > 0 && geometry2.BoundingBoxArea > 0)
                {
                    var areaDiff = Math.Abs((double)geometry1.BoundingBoxArea - (double)geometry2.BoundingBoxArea);
                    var maxArea = Math.Max((double)geometry1.BoundingBoxArea, (double)geometry2.BoundingBoxArea);
                    var areaSim = Math.Max(0, 100 - (areaDiff / maxArea * 100));
                    similarities.Add(areaSim);
                }

                // Center position similarity (relative)
                var centerXDiff = Math.Abs((double)geometry1.CenterX - (double)geometry2.CenterX);
                var centerYDiff = Math.Abs((double)geometry1.CenterY - (double)geometry2.CenterY);
                var centerSim = Math.Max(0, 100 - Math.Sqrt(centerXDiff * centerXDiff + centerYDiff * centerYDiff));
                similarities.Add(centerSim);

                // Spread similarity
                if (geometry1.Spread > 0 && geometry2.Spread > 0)
                {
                    var spreadDiff = Math.Abs((double)geometry1.Spread - (double)geometry2.Spread);
                    var maxSpread = Math.Max((double)geometry1.Spread, (double)geometry2.Spread);
                    var spreadSim = Math.Max(0, 100 - (spreadDiff / maxSpread * 100));
                    similarities.Add(spreadSim);
                }

                return similarities.Any() ? similarities.Average() : 50;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating shape similarity");
                return 0;
            }
        }

        private double CalculateTimingSimilarity(TokenSignature signature1, TokenSignature signature2)
        {
            try
            {
                // For now, return neutral score as timing data is not fully implemented
                // This can be enhanced when timing patterns are added to the signature
                return 50;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating timing similarity");
                return 0;
            }
        }

        private double CalculateGeometricSimilarity(TokenSignature signature1, TokenSignature signature2)
        {
            try
            {
                var geometry1 = signature1.TouchProperties;
                var geometry2 = signature2.TouchProperties;

                if (geometry1 == null || geometry2 == null)
                    return 50;

                var similarities = new List<double>();

                // Radius similarity
                if (geometry1.HasRadius && geometry2.HasRadius)
                {
                    var radiusDiff = Math.Abs((double)geometry1.AvgRadius - (double)geometry2.AvgRadius);
                    var maxRadius = Math.Max(Math.Max((double)geometry1.AvgRadius, (double)geometry2.AvgRadius), 1);
                    var radiusSim = Math.Max(0, 100 - (radiusDiff / maxRadius * 100));
                    similarities.Add(radiusSim);
                }

                // Rotation similarity
                if (geometry1.HasRotation && geometry2.HasRotation)
                {
                    var rotationDiff = Math.Abs((double)geometry1.AvgRotation - (double)geometry2.AvgRotation);
                    var rotationSim = Math.Max(0, 100 - (rotationDiff / 180 * 100));
                    similarities.Add(rotationSim);
                }

                return similarities.Any() ? similarities.Average() : 50;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating geometric similarity");
                return 0;
            }
        }

        public async Task<PatternAnalysisResult> AnalyzePatternAsync(TokenSignature signature)
        {
            try
            {
                var result = new PatternAnalysisResult
                {
                    PatternType = signature.TouchCount == 1 ? "single" : "multi",
                    Complexity = signature.TouchCount,
                    Confidence = 0,
                    Characteristics = new List<string>(),
                    Metrics = new Dictionary<string, object>()
                };

                // Analyze touch pattern
                if (signature.TouchPattern != null)
                {
                    result.Characteristics.Add($"Touch count: {signature.TouchCount}");
                    result.Characteristics.Add($"Pattern type: {signature.TouchPattern.Type}");
                    result.Characteristics.Add($"Complexity: {signature.TouchPattern.Complexity}");

                    if (signature.TouchPattern.Distances != null)
                    {
                        var distances = JsonSerializer.Deserialize<double[]>(signature.TouchPattern.Distances) ?? new double[0];
                        if (distances.Length > 0)
                        {
                            result.Metrics["AverageDistance"] = distances.Average();
                            result.Metrics["MinDistance"] = distances.Min();
                            result.Metrics["MaxDistance"] = distances.Max();
                            result.Metrics["DistanceVariance"] = CalculateVariance(distances);
                        }
                    }
                }

                // Analyze multi-touch geometry
                if (signature.MultiTouchGeometry != null)
                {
                    var geometry = signature.MultiTouchGeometry;
                    result.Characteristics.Add($"Aspect ratio: {(double)geometry.AspectRatio:F2}");
                    result.Characteristics.Add($"Bounding box: {(double)geometry.BoundingBoxWidth:F1}x{(double)geometry.BoundingBoxHeight:F1}");
                    result.Characteristics.Add($"Area: {(double)geometry.BoundingBoxArea:F1}");
                    result.Characteristics.Add($"Spread: {(double)geometry.Spread:F1}");
                    result.Characteristics.Add($"Density: {(double)geometry.Density:F2}");

                    result.Metrics["AspectRatio"] = (double)geometry.AspectRatio;
                    result.Metrics["BoundingBoxArea"] = (double)geometry.BoundingBoxArea;
                    result.Metrics["Spread"] = (double)geometry.Spread;
                    result.Metrics["Density"] = (double)geometry.Density;
                }

                // Analyze touch properties
                if (signature.TouchProperties != null)
                {
                    var properties = signature.TouchProperties;
                    if (properties.HasRadius)
                    {
                        result.Characteristics.Add($"Average radius: {(double)properties.AvgRadius:F2}");
                        result.Metrics["AvgRadius"] = (double)properties.AvgRadius;
                        result.Metrics["RadiusVariance"] = (double)properties.RadiusVariance;
                    }

                    if (properties.HasRotation)
                    {
                        result.Characteristics.Add($"Average rotation: {(double)properties.AvgRotation:F1}°");
                        result.Metrics["AvgRotation"] = (double)properties.AvgRotation;
                    }
                }

                // Calculate overall confidence
                result.Confidence = CalculatePatternConfidence(signature);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing pattern");
                return new PatternAnalysisResult
                {
                    PatternType = "unknown",
                    Complexity = 0,
                    Confidence = 0,
                    Characteristics = new List<string> { "Error analyzing pattern" },
                    Metrics = new Dictionary<string, object>()
                };
            }
        }

        private double CalculatePatternConfidence(TokenSignature signature)
        {
            try
            {
                var confidence = 0.0;
                var factors = 0;

                // Touch count factor
                if (signature.TouchCount > 0)
                {
                    confidence += Math.Min(signature.TouchCount * 10.0, 50.0);
                    factors++;
                }

                // Pattern complexity factor
                if (signature.TouchPattern != null)
                {
                    confidence += Math.Min(signature.TouchPattern.Complexity * 5.0, 30.0);
                    factors++;
                }

                // Geometry factor
                if (signature.MultiTouchGeometry != null)
                {
                    confidence += 20;
                    factors++;
                }

                // Properties factor
                if (signature.TouchProperties != null)
                {
                    confidence += 10;
                    factors++;
                }

                return factors > 0 ? Math.Min(confidence / factors * 2.0, 100.0) : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating pattern confidence");
                return 0;
            }
        }

        private double CalculateVariance(double[] values)
        {
            if (values.Length == 0) return 0;

            var mean = values.Average();
            var squaredDiffs = values.Select(v => Math.Pow(v - mean, 2));
            return squaredDiffs.Average();
        }
    }
}
