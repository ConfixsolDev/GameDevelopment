using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TechWebSol.Models
{
    [Table("Tokens")]
    public class Token:BaseEntity
    {

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        public decimal TrainingConsistency { get; set; }

        // Navigation property
        public virtual TokenSignature? Signature { get; set; }

        // Additional metadata - removed description and category as discussed

        public bool IsManualToken { get; set; } = false;

        public DateTime? LastUsed { get; set; }

        public int UsageCount { get; set; } = 0;
        [MaxLength(1000)]
        public string? Notes { get; set; }
        public Guid? TokenGroupId { get; set; }
        public virtual TokenGroup? TokenGroup { get; set; }
        public virtual ICollection<MapMarker> MapMarkers { get; set; } = new List<MapMarker>();

        // Asset properties for military tokens
        [MaxLength(200)]
        public string? AssetImagePath { get; set; } // Path to asset image/insignia

        [Column(TypeName = "decimal(8,2)")]
        public decimal? CoverageRadiusKm { get; set; } // Asset coverage radius

        // Position is tracked via MapMarkers (single active marker indicates current position)

        // Navigation properties
        public virtual ICollection<TokenAreaCoverage> AreaCoverages { get; set; } = new List<TokenAreaCoverage>();
    }

    // DTOs for pattern matching
    public class GeometricData
    {
        public double[] Distances { get; set; } = Array.Empty<double>();
        public double[] Angles { get; set; } = Array.Empty<double>();
        public CenterPoint Center { get; set; } = new CenterPoint();
        public int TouchCount { get; set; }
    }

    public class CenterPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class PatternAnalysisResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public List<PatternMetric> Metrics { get; set; } = new();
        public string Characteristics { get; set; } = string.Empty;
        public string Complexity { get; set; } = string.Empty;
        public string PatternType { get; set; } = string.Empty;
    }

    public class PatternMetric
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public double Weight { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Unit { get; set; } = string.Empty;
    }

    public class PatternSimilarityResult
    {
        public double DistanceSimilarity { get; set; }
        public double AngleSimilarity { get; set; }
        public double CenterSimilarity { get; set; }
        public double ShapeSimilarity { get; set; }
        public double TimingSimilarity { get; set; }
        public double GeometricSimilarity { get; set; }
        public double OverallSimilarity { get; set; }
        public Dictionary<string, double> MatchFactors { get; set; } = new();
    }

    public class PatternStatistics
    {
        public int TotalPatterns { get; set; }
        public double AverageConfidence { get; set; }
        public double SuccessRate { get; set; }
        public Guid? TokenId { get; set; }
        public string TokenName { get; set; } = string.Empty;
        public int TotalIdentifications { get; set; }
        public int SuccessfulIdentifications { get; set; }
        public DateTime LastIdentified { get; set; }
        public List<PatternMetric> Metrics { get; set; } = new();
    }

    public class TokenIdentificationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public Token? MatchedToken { get; set; }
        public List<TokenMatchDetail> AllMatches { get; set; } = new();
    }

    public class TokenMatchDetail
    {
        public Guid? TokenId { get; set; }
        public string TokenName { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public double DistanceSimilarity { get; set; }
        public double AngleSimilarity { get; set; }
        public double CenterSimilarity { get; set; }
        public double ShapeSimilarity { get; set; }
        public double TimingSimilarity { get; set; }
        public double GeometricSimilarity { get; set; }
        public double OverallSimilarity { get; set; }
        public Dictionary<string, double> MatchFactors { get; set; } = new();
    }

    public class TokenStatistics
    {
        public int TotalTokens { get; set; }
        public int ActiveTokens { get; set; }
        public int InactiveTokens { get; set; }
        public double AverageUsage { get; set; }
        public double AverageConfidence { get; set; }
        public DateTime? LastTokenCreated { get; set; }
        public DateTime? LastTokenUsed { get; set; }
        public int TotalIdentifications { get; set; }
        public int SuccessfulIdentifications { get; set; }
        public double SuccessRate { get; set; }
    }

    public class ComplexTokenIdentificationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public Token? MatchedToken { get; set; }
        public List<TokenMatch> AllMatches { get; set; } = new();
    }

    public class TokenMatch
    {
        public Guid? TokenId { get; set; }
        public string TokenName { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public double DistanceSimilarity { get; set; }
        public double ShapeSimilarity { get; set; }
        public double TimingSimilarity { get; set; }
        public double GeometricSimilarity { get; set; }
        public Dictionary<string, double> MatchFactors { get; set; } = new();
    }

    public class ComplexTokenMatchDetail
    {
        public Guid? TokenId { get; set; }
        public string TokenName { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public double DistanceSimilarity { get; set; }
        public double ShapeSimilarity { get; set; }
        public double TimingSimilarity { get; set; }
        public double GeometricSimilarity { get; set; }
        public Dictionary<string, double> MatchFactors { get; set; } = new();
    }

    [Table("TokenSignatures")]
    public class TokenSignature : BaseEntity
    {
        [ForeignKey("Token")]
        public Guid TokenId { get; set; }

        public int TouchCount { get; set; }

        public long Timestamp { get; set; }

        // Navigation properties
        public virtual Token Token { get; set; } = null!;
        public virtual StabilityInfo? Stability { get; set; }
        public virtual TouchGeometry? TouchProperties { get; set; }
        public virtual TouchPattern? TouchPattern { get; set; }
        public virtual MultiTouchGeometry? MultiTouchGeometry { get; set; }

        [MaxLength(500)]
        public string? TokenHash { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? OriginalTouches { get; set; }

        // Simplified properties for basic pattern matching
        [Column(TypeName = "nvarchar(max)")]
        public string? Distances { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Angles { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Center { get; set; }
    }

    [Table("StabilityInfo")]
    public class StabilityInfo
    {
        [Key]
        [ForeignKey("TokenSignature")]
        public Guid TokenSignatureId { get; set; }  // reuse signature ID as primary key

        public bool IsStabilized { get; set; }

        public long GeneratedAt { get; set; }

        public int SampleCount { get; set; }

        public virtual TokenSignature TokenSignature { get; set; } = null!;
    }

    [Table("TouchGeometry")]
    public class TouchGeometry
    {
        [Key]
        [ForeignKey("TokenSignature")]
        public Guid TokenSignatureId { get; set; }

        public bool HasRadius { get; set; }

        public bool HasRotation { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? RadiusValues { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? RotationValues { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal AvgRadius { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal AvgRotation { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal RadiusVariance { get; set; }

        public virtual TokenSignature TokenSignature { get; set; } = null!;
    }

    [Table("TouchPatterns")]
    public class TouchPattern
    {
        [Key]
        [ForeignKey("TokenSignature")]
        public Guid TokenSignatureId { get; set; }

        [MaxLength(20)]
        public string Type { get; set; } = string.Empty;

        public int Complexity { get; set; }

        [Column(TypeName = "nvarchar(max)")]

        public string? Distances { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? DistancePairs { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal AvgDistance { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal MinDistance { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal MaxDistance { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal DistanceRange { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal DistanceVariance { get; set; }

        [MaxLength(500)]
        public string? DistanceSignature { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal AngleSpread { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? GeometricCenter { get; set; }

        public virtual TokenSignature TokenSignature { get; set; } = null!;
    }

    [Table("MultiTouchGeometry")]
    public class MultiTouchGeometry
    {
        [Key]
        [ForeignKey("TokenSignature")]
        public Guid TokenSignatureId { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal AspectRatio { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal BoundingBoxWidth { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal BoundingBoxHeight { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal BoundingBoxArea { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal CenterX { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal CenterY { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal Spread { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal Density { get; set; }

        public virtual TokenSignature TokenSignature { get; set; } = null!;
    }


}
