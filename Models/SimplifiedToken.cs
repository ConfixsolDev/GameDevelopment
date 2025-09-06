using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TechWebSol.Models
{
    /// <summary>
    /// Simplified Token model - only essential properties for geometric pattern matching
    /// Supports 2-5 touch points with distance and angle calculations
    /// </summary>
    public class SimplifiedToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }  // pre-generated timestamp ID

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastUsed { get; set; }

        public int UsageCount { get; set; } = 0;

        // Navigation property
        public virtual SimplifiedTokenSignature? Signature { get; set; }

        // Optional metadata
        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }
    }

    /// <summary>
    /// Simplified TokenSignature - only geometric properties needed for pattern matching
    /// Stores distances between all points, angles formed, and center point
    /// </summary>
    public class SimplifiedTokenSignature
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("SimplifiedToken")]
        public long TokenId { get; set; }

        /// <summary>
        /// Number of touch points (2, 3, 4, or 5)
        /// </summary>
        public int TouchCount { get; set; }

        /// <summary>
        /// JSON array of distances between all touch points
        /// For 2 points: [A-B]
        /// For 3 points: [A-B, B-C, C-A] 
        /// For 4 points: [A-B, A-C, A-D, B-C, B-D, C-D]
        /// For 5 points: [A-B, A-C, A-D, A-E, B-C, B-D, B-E, C-D, C-E, D-E]
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string Distances { get; set; } = string.Empty;

        /// <summary>
        /// JSON array of angles formed by the shape
        /// For 2 points: [] (no angles)
        /// For 3 points: [angle at A, angle at B, angle at C]
        /// For 4 points: [angle at A, angle at B, angle at C, angle at D]
        /// For 5 points: [angle at A, angle at B, angle at C, angle at D, angle at E]
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string Angles { get; set; } = string.Empty;

        /// <summary>
        /// JSON object with center point coordinates {X: double, Y: double}
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string Center { get; set; } = string.Empty;

        /// <summary>
        /// When this signature was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public virtual SimplifiedToken Token { get; set; } = null!;
    }

    /// <summary>
    /// Helper class for center point coordinates
    /// </summary>
    public class CenterPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    /// <summary>
    /// Helper class for distance and angle data
    /// </summary>
    public class GeometricData
    {
        public double[] Distances { get; set; } = Array.Empty<double>();
        public double[] Angles { get; set; } = Array.Empty<double>();
        public CenterPoint Center { get; set; } = new CenterPoint();
    }

    /// <summary>
    /// Result of token identification
    /// </summary>
    public class TokenIdentificationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SimplifiedToken? MatchedToken { get; set; }
        public double Confidence { get; set; }
        public List<TokenMatchDetail> AllMatches { get; set; } = new();
    }

    /// <summary>
    /// Details of a token match
    /// </summary>
    public class TokenMatchDetail
    {
        public long TokenId { get; set; }
        public string TokenName { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public double DistanceSimilarity { get; set; }
        public double ShapeSimilarity { get; set; }
        public double TimingSimilarity { get; set; }
        public double GeometricSimilarity { get; set; }
        public List<string> MatchFactors { get; set; } = new();
    }

    /// <summary>
    /// Pattern similarity result
    /// </summary>
    public class PatternSimilarityResult
    {
        public double OverallSimilarity { get; set; }
        public double DistanceSimilarity { get; set; }
        public double ShapeSimilarity { get; set; }
        public double TimingSimilarity { get; set; }
        public double GeometricSimilarity { get; set; }
        public List<string> MatchFactors { get; set; } = new();
    }

    /// <summary>
    /// Pattern analysis result
    /// </summary>
    public class PatternAnalysisResult
    {
        public string PatternType { get; set; } = string.Empty;
        public int Complexity { get; set; }
        public double Confidence { get; set; }
        public List<string> Characteristics { get; set; } = new();
        public Dictionary<string, object> Metrics { get; set; } = new();
    }

    /// <summary>
    /// Pattern statistics
    /// </summary>
    public class PatternStatistics
    {
        public long TokenId { get; set; }
        public string TokenName { get; set; } = string.Empty;
        public int TotalIdentifications { get; set; }
        public int SuccessfulIdentifications { get; set; }
        public double AverageConfidence { get; set; }
        public double SuccessRate { get; set; }
        public DateTime LastIdentified { get; set; }
        public List<PatternMetric> Metrics { get; set; } = new();
    }

    /// <summary>
    /// Pattern metric
    /// </summary>
    public class PatternMetric
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Token statistics
    /// </summary>
    public class TokenStatistics
    {
        public int TotalTokens { get; set; }
        public int ActiveTokens { get; set; }
        public int InactiveTokens { get; set; }
        public DateTime? LastTokenCreated { get; set; }
        public DateTime? LastTokenUsed { get; set; }
        public double AverageConfidence { get; set; }
        public int TotalIdentifications { get; set; }
        public int SuccessfulIdentifications { get; set; }
        public double SuccessRate { get; set; }
    }

    /// <summary>
    /// Token identification request for simplified pattern matching
    /// </summary>
    public class TokenIdentificationRequest
    {
        public GeometricData Signature { get; set; } = null!;
        public double? ConfidenceThreshold { get; set; }
    }

    /// <summary>
    /// Pattern similarity request for simplified pattern matching
    /// </summary>
    public class PatternSimilarityRequest
    {
        public GeometricData Signature1 { get; set; } = null!;
        public GeometricData Signature2 { get; set; } = null!;
    }

    /// <summary>
    /// Touch points request for calculating geometric data
    /// </summary>
    public class TouchPointsRequest
    {
        public double[][] TouchPoints { get; set; } = null!;
    }
}
