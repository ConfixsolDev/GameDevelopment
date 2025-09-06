using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Models;

namespace TechWebSol.Services.TokenManagement
{
    public class PatternMatchingService : IPatternMatchingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PatternMatchingService> _logger;
        private readonly PatternAnalysisEngine _analysisEngine;

        public PatternMatchingService(ApplicationDbContext context, ILogger<PatternMatchingService> logger, PatternAnalysisEngine analysisEngine)
        {
            _context = context;
            _logger = logger;
            _analysisEngine = analysisEngine;
        }

        public async Task<ComplexTokenIdentificationResult> IdentifyTokenAsync(TokenSignature currentSignature, double confidenceThreshold = 70.0)
        {
            try
            {
                _logger.LogInformation("Starting token identification for signature with {TouchCount} touches", 
                    currentSignature.TouchCount);

                // Get all active tokens with their signatures
                var tokens = await _context.Tokens
                    .Include(t => t.Signature)
                        .ThenInclude(s => s.Stability)
                    .Include(t => t.Signature)
                        .ThenInclude(s => s.TouchProperties)
                    .Include(t => t.Signature)
                        .ThenInclude(s => s.TouchPattern)
                    .Include(t => t.Signature)
                        .ThenInclude(s => s.MultiTouchGeometry)
                    .Where(t => t.IsActive && t.Signature != null)
                    .ToListAsync();

                if (!tokens.Any())
                {
                    return new ComplexTokenIdentificationResult
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

                var matchDetails = new List<ComplexTokenMatchDetail>();

                // Compare with each stored token
                foreach (var token in tokens)
                {
                    var similarityResult = await _analysisEngine.CalculatePatternSimilarityAsync(currentSignature, token.Signature);
                    
                    var matchDetail = new ComplexTokenMatchDetail
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

                return new ComplexTokenIdentificationResult
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
                _logger.LogError(ex, "Error during token identification");
                return new ComplexTokenIdentificationResult
                {
                    Success = false,
                    Message = "Error during token identification"
                };
            }
        }

        public async Task<PatternSimilarityResult> CalculatePatternSimilarityAsync(TokenSignature signature1, TokenSignature signature2)
        {
            try
            {
                return await _analysisEngine.CalculatePatternSimilarityAsync(signature1, signature2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating pattern similarity");
                return new PatternSimilarityResult { OverallSimilarity = 0 };
            }
        }

        public async Task<PatternAnalysisResult> AnalyzePatternAsync(TokenSignature signature)
        {
            try
            {
                return await _analysisEngine.AnalyzePatternAsync(signature);
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

        public async Task<bool> ValidatePatternConsistencyAsync(List<TokenSignature> trainingSignatures)
        {
            try
            {
                if (trainingSignatures.Count < 2)
                    return false;

                var firstSignature = trainingSignatures[0];
                var consistencyScores = new List<double>();

                for (int i = 1; i < trainingSignatures.Count; i++)
                {
                    var similarity = await _analysisEngine.CalculatePatternSimilarityAsync(firstSignature, trainingSignatures[i]);
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

        public async Task<PatternStatistics> GetPatternStatisticsAsync(long tokenId)
        {
            try
            {
                var token = await _context.Tokens
                    .Include(t => t.Signature)
                    .FirstOrDefaultAsync(t => t.Id == tokenId);

                if (token == null)
                {
                    return new PatternStatistics
                    {
                        TokenId = tokenId,
                        TokenName = "Unknown",
                        TotalIdentifications = 0,
                        SuccessfulIdentifications = 0,
                        AverageConfidence = 0,
                        SuccessRate = 0,
                        LastIdentified = DateTime.MinValue
                    };
                }

                // Calculate statistics based on usage data
                var totalIdentifications = token.UsageCount;
                var successfulIdentifications = totalIdentifications; // Assuming all usage counts are successful
                var successRate = totalIdentifications > 0 ? 100.0 : 0.0;
                var averageConfidence = 85.0; // Placeholder - would need to track actual confidence scores

                var metrics = new List<PatternMetric>
                {
                    new PatternMetric
                    {
                        Name = "Touch Count",
                        Value = token.Signature?.TouchCount ?? 0,
                        Unit = "touches",
                        Timestamp = DateTime.UtcNow
                    },
                    new PatternMetric
                    {
                        Name = "Training Consistency",
                        Value = (double)token.TrainingConsistency,
                        Unit = "%",
                        Timestamp = DateTime.UtcNow
                    },
                    new PatternMetric
                    {
                        Name = "Usage Count",
                        Value = token.UsageCount,
                        Unit = "identifications",
                        Timestamp = DateTime.UtcNow
                    }
                };

                return new PatternStatistics
                {
                    TokenId = token.Id,
                    TokenName = token.Name,
                    TotalIdentifications = totalIdentifications,
                    SuccessfulIdentifications = successfulIdentifications,
                    AverageConfidence = averageConfidence,
                    SuccessRate = successRate,
                    LastIdentified = token.LastUsed ?? DateTime.MinValue,
                    Metrics = metrics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pattern statistics for token {TokenId}", tokenId);
                return new PatternStatistics
                {
                    TokenId = tokenId,
                    TokenName = "Error",
                    TotalIdentifications = 0,
                    SuccessfulIdentifications = 0,
                    AverageConfidence = 0,
                    SuccessRate = 0,
                    LastIdentified = DateTime.MinValue
                };
            }
        }

        private class TokenMatch
        {
            public double Confidence { get; set; }
            public Token? Token { get; set; }
            public ComplexTokenMatchDetail? MatchDetails { get; set; }
        }
    }
}
