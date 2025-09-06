using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TechWebSol.Models
{
    [Table("Tokens")]
    public class Token
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }  // pre-generated ID (e.g. 1757013297849L)

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TrainingConsistency { get; set; }

        // Navigation property
        public virtual TokenSignature? Signature { get; set; }

        // Additional metadata
        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastUsed { get; set; }

        public int UsageCount { get; set; } = 0;

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Team isolation - tokens are team-specific
        [Required]
        [MaxLength(50)]
        public string TeamId { get; set; } = string.Empty; // TeamCode + SubTeamCode

        [Required]
        [MaxLength(50)]
        public string CreatedByUserId { get; set; } = string.Empty;

        // Token group assignment - tokens belong to administrator-managed groups
        public int? TokenGroupId { get; set; }

        // Navigation property
        public virtual TokenGroup? TokenGroup { get; set; }

        public virtual ICollection<MapMarker> MapMarkers { get; set; } = new List<MapMarker>();
    }

    [Table("TokenSignatures")]
    public class TokenSignature
    {
        [Key]
        public int Id { get; set; }  

        [ForeignKey("Token")]
        public long TokenId { get; set; }

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
    }

    [Table("StabilityInfo")]
    public class StabilityInfo
    {
        [Key]
        [ForeignKey("TokenSignature")]
        public int TokenSignatureId { get; set; }  // reuse signature ID as primary key

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
        public int TokenSignatureId { get; set; }

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
        public int TokenSignatureId { get; set; }

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
        public int TokenSignatureId { get; set; }

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

    /// <summary>
    /// Result of token identification for complex system
    /// </summary>
    public class ComplexTokenIdentificationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Token? MatchedToken { get; set; }
        public double Confidence { get; set; }
        public List<ComplexTokenMatchDetail> AllMatches { get; set; } = new();
    }

    /// <summary>
    /// Details of a token match for complex system
    /// </summary>
    public class ComplexTokenMatchDetail
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

}
