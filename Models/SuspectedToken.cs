using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    /// <summary>
    /// Represents a suspected enemy token/contact (fog of war intelligence)
    /// Used by one side to track suspected positions of enemy forces
    /// </summary>
    [Table("SuspectedTokens")]
    public class SuspectedToken : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The side that placed this suspected token (e.g., "Blue Land")
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string PlacerSide { get; set; } = string.Empty;

        /// <summary>
        /// Latitude of suspected position
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,7)")]
        public decimal Latitude { get; set; }

        /// <summary>
        /// Longitude of suspected position
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,7)")]
        public decimal Longitude { get; set; }

        /// <summary>
        /// Source of intelligence (e.g., "radar", "uav", "human", "satellite")
        /// </summary>
        [MaxLength(50)]
        public string? Source { get; set; }

        /// <summary>
        /// Confidence level (0-100)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal Confidence { get; set; } = 40;

        /// <summary>
        /// Status: suspected, confirmed, dismissed
        /// </summary>
        [MaxLength(50)]
        public string Status { get; set; } = "suspected";

        /// <summary>
        /// Additional notes about this suspected contact
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// When this suspected contact was first detected
        /// </summary>
        public DateTime? FirstDetectedAt { get; set; }

        /// <summary>
        /// Last time this suspected contact was updated/confirmed
        /// </summary>
        public DateTime? LastConfirmedAt { get; set; }

        /// <summary>
        /// Suspected unit type (if known)
        /// </summary>
        [MaxLength(100)]
        public string? SuspectedType { get; set; }

        /// <summary>
        /// Icon or marker style for this suspected token
        /// </summary>
        [MaxLength(50)]
        public string? MarkerStyle { get; set; } = "question-mark";

        [MaxLength(50)]
        public string? VisibleTo { get; set; }

        /// <summary>
        /// Related ISR missions for this suspected token
        /// </summary>
        public virtual ICollection<ISRMission>? ISRMissions { get; set; }

        /// <summary>
        /// The actual real token this suspected contact represents (known only to system for simulation)
        /// Automatically matched when suspected token is placed or updated
        /// </summary>
        public Guid? RealTokenId { get; set; }

        [ForeignKey("RealTokenId")]
        public virtual Token? RealToken { get; set; }

        /// <summary>
        /// Position accuracy in meters (how close the suspected position is to real token)
        /// </summary>
        public int? PositionAccuracyMeters { get; set; }

        /// <summary>
        /// Confidence score of the automatic matching (0-100)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? MatchingConfidence { get; set; }
    }

    /// <summary>
    /// Represents an Intelligence, Surveillance, and Reconnaissance mission
    /// Used to improve confidence in suspected tokens
    /// </summary>
    [Table("ISRMissions")]
    public class ISRMission : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The suspected token being investigated
        /// </summary>
        public Guid? SuspectedTokenId { get; set; }
        public virtual SuspectedToken? SuspectedToken { get; set; }

        /// <summary>
        /// Type of ISR asset (uav, satellite, radar, patrol, etc.)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string AssetType { get; set; } = "uav";

        /// <summary>
        /// Mission start time
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Mission end time
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Confidence gain from this mission (0-100)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal ConfidenceGain { get; set; } = 20;

        /// <summary>
        /// Fuel/resource cost for this mission
        /// </summary>
        [Column(TypeName = "decimal(8,2)")]
        public decimal? CostFuel { get; set; }

        /// <summary>
        /// Exposure risk (0-1)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? ExposureRisk { get; set; }

        /// <summary>
        /// Who requested this mission
        /// </summary>
        [MaxLength(100)]
        public string? RequestedBy { get; set; }

        /// <summary>
        /// Mission status: scheduled, in-progress, completed, cancelled
        /// </summary>
        [MaxLength(50)]
        public string Status { get; set; } = "scheduled";

        /// <summary>
        /// Mission results/notes
        /// </summary>
        [MaxLength(1000)]
        public string? Results { get; set; }
    }
}

