using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    /// <summary>
    /// Represents a planned or executed attack order between two tokens
    /// </summary>
    [Table("AttackOrders")]
    public class AttackOrder : BaseEntity
    {
        [Required]
        public Guid AttackerTokenId { get; set; }

        [Required]
        public Guid TargetTokenId { get; set; }


        [MaxLength(50)]
        public string? AxisId { get; set; }

        /// <summary>
        /// JSON array of artillery unit IDs attached to this attack
        /// </summary>
        [MaxLength(1000)]
        public string? ArtilleryAttached { get; set; }

        /// <summary>
        /// Movement points reserve percentage (0.0 to 0.5)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal MpReservePercent { get; set; } = 0.1m;

        [MaxLength(20)]
        public string Posture { get; set; } = "Advance"; // Advance, Fix, Feint

        public int ExpectedStartTurn { get; set; }

        public int DurationTurns { get; set; } = 1;

        [MaxLength(20)]
        public string ExecutionMode { get; set; } = "Plan"; // Plan, ExecuteNow

        [MaxLength(20)]
        public string Status { get; set; } = "Planned"; // Planned, Executing, Completed, Failed, Cancelled

        public DateTime? ExecutedUtc { get; set; }

        public DateTime? LastUpdated { get; set; }

        /// <summary>
        /// Additional attack parameters and results in JSON format
        /// </summary>
        [MaxLength(2000)]
        public string? PayloadJson { get; set; }

        /// <summary>
        /// Detection confidence when order was planned (0.0 to 1.0)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal DetectionConfidence { get; set; } = 1.0m;

        /// <summary>
        /// Whether this attack was planned with low confidence due to fog of war
        /// </summary>
        public bool IsLowConfidence { get; set; } = false;

        // Navigation properties
        [ForeignKey("AttackerTokenId")]
        public virtual Token? AttackerToken { get; set; }

        [ForeignKey("TargetTokenId")]
        public virtual Token? TargetToken { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team? Team { get; set; }
    }

    /// <summary>
    /// Result of attack preview calculation
    /// </summary>
    public class AttackPreviewResult
    {
        public double DetectionConfidence { get; set; }
        public double AttackerEffectiveCombatPower { get; set; }
        public double DefenderEffectiveCombatPowerEstimated { get; set; }
        public double AttackerExpectedCasualtyPercent { get; set; }
        public double DefenderExpectedCasualtyPercent { get; set; }
        public double ProbabilityOfSuccess { get; set; }
        public double MovementNeededKm { get; set; }
        public double MpShortfall { get; set; }
        public List<string> SupplyWarnings { get; set; } = new List<string>();
        public string UncertaintyNotes { get; set; } = string.Empty;
        public bool CanTarget { get; set; } = true;
        public string AttackerTokenName { get; set; } = string.Empty;
        public string TargetTokenName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for planning an attack
    /// </summary>
    public class PlanAttackRequest
    {
        [Required]
        public string AttackerId { get; set; } = string.Empty;

        [Required]
        public string TargetId { get; set; } = string.Empty;

        public string? AxisId { get; set; }

        public List<string> ArtilleryAttached { get; set; } = new List<string>();

        [Range(0.0, 0.5)]
        public double MpReservePercent { get; set; } = 0.1;

        public string Posture { get; set; } = "Advance";

        public int ExpectedStartTurn { get; set; }

        public int DurationTurns { get; set; } = 1;

        public string ExecutionMode { get; set; } = "Plan";

        public string? Notes { get; set; }
    }

    /// <summary>
    /// Response model for attack planning
    /// </summary>
    public class PlanAttackResponse
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public AttackPreviewResult? Preview { get; set; }
    }
}
