using TechWebSol.Models;

namespace TechWebSol.Services.TokenManagement
{
    /// <summary>
    /// Simplified pattern matching service interface
    /// Focuses only on geometric properties: distances, angles, and center point
    /// </summary>
    public interface ISimplifiedPatternMatchingService
    {
        Task<TokenIdentificationResult> IdentifyTokenAsync(GeometricData currentData, double confidenceThreshold = 70.0);
        Task<PatternSimilarityResult> CalculatePatternSimilarityAsync(GeometricData data1, GeometricData data2);
        Task<bool> ValidatePatternConsistencyAsync(List<GeometricData> trainingData);
        Task<GeometricData> CalculateGeometricDataAsync(double[][] touchPoints);
    }
}
