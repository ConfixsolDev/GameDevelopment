using TechWebSol.Models;

namespace TechWebSol.Services.TokenManagement
{
    public interface IPatternMatchingService
    {
        Task<ComplexTokenIdentificationResult> IdentifyTokenAsync(TokenSignature currentSignature, double confidenceThreshold = 70.0);
        Task<PatternSimilarityResult> CalculatePatternSimilarityAsync(TokenSignature signature1, TokenSignature signature2);
        Task<PatternAnalysisResult> AnalyzePatternAsync(TokenSignature signature);
        Task<bool> ValidatePatternConsistencyAsync(List<TokenSignature> trainingSignatures);
        Task<PatternStatistics> GetPatternStatisticsAsync(long tokenId);
    }

}
