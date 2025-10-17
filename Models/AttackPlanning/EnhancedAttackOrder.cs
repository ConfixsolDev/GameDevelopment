using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models.AttackPlanning
{
    /// <summary>
    /// Enhanced attack order with comprehensive planning parameters
    /// </summary>
    [Table("EnhancedAttackOrders")]
    public class EnhancedAttackOrder : BaseEntity
    {
        [Required]
        [Display(Name = "Attacker Token ID")]
        public Guid AttackerTokenId { get; set; }

        [Required]
        [Display(Name = "Target Token ID")]
        public Guid TargetTokenId { get; set; }

        // Complex planning objects (stored as JSON)
        [Column(TypeName = "nvarchar(max)")]
        public string? IntentJson { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? FiresJson { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? MovementJson { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? TimingJson { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? FogOfWarJson { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? LogisticsJson { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? ROEJson { get; set; }

        // Status and execution
        [Required]
        [MaxLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Draft"; // "Draft", "Planned", "Approved", "Executing", "Completed", "Failed", "Cancelled"

        [Display(Name = "Execution Mode")]
        [MaxLength(20)]
        public string ExecutionMode { get; set; } = "Plan"; // "Plan", "ExecuteNow"

        [Display(Name = "Executed Date")]
        public DateTime? ExecutedUtc { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime? LastUpdated { get; set; }

        [Display(Name = "Execution Notes")]
        [MaxLength(1000)]
        public string? ExecutionNotes { get; set; }

        // Navigation properties for non-JSON access
        [NotMapped]
        public AttackIntent? Intent { get; set; }

        [NotMapped]
        public FiresSupport? Fires { get; set; }

        [NotMapped]
        public AttackMovement? Movement { get; set; }

        [NotMapped]
        public AttackTiming? Timing { get; set; }

        [NotMapped]
        public FogOfWar? FogOfWar { get; set; }

        [NotMapped]
        public AttackLogistics? Logistics { get; set; }

        [NotMapped]
        public RulesOfEngagement? ROE { get; set; }

        // Calculated properties
        [NotMapped]
        public bool IsComplete => !string.IsNullOrEmpty(IntentJson) && 
                                  !string.IsNullOrEmpty(TimingJson) && 
                                  !string.IsNullOrEmpty(MovementJson);

        [NotMapped]
        public bool IsExecutable => IsComplete && Status == "Approved";

        [NotMapped]
        public string CompletionPercentage
        {
            get
            {
                int completed = 0;
                int total = 7; // Total planning sections

                if (!string.IsNullOrEmpty(IntentJson)) completed++;
                if (!string.IsNullOrEmpty(FiresJson)) completed++;
                if (!string.IsNullOrEmpty(MovementJson)) completed++;
                if (!string.IsNullOrEmpty(TimingJson)) completed++;
                if (!string.IsNullOrEmpty(FogOfWarJson)) completed++;
                if (!string.IsNullOrEmpty(LogisticsJson)) completed++;
                if (!string.IsNullOrEmpty(ROEJson)) completed++;

                return $"{Math.Round((double)completed / total * 100)}%";
            }
        }

        // Static options
        public static readonly string[] Statuses = { "Draft", "Planned", "Approved", "Executing", "Completed", "Failed", "Cancelled" };
        public static readonly string[] ExecutionModes = { "Plan", "ExecuteNow" };
    }
}
