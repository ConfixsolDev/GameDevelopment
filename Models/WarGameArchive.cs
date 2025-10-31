using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    /// <summary>
    /// Stores complete war game state and results in a single denormalized table
    /// Includes map overlays, forces, attacks, maneuvers, and game turns
    /// </summary>
    [Table("WarGameArchives")]
    public class WarGameArchive : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string GameTitle { get; set; } = string.Empty; // e.g., "Operation Desert Storm 2024"

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string GameCode { get; set; } = string.Empty; // e.g., "WG202401"

        /// <summary>
        /// Month and year when the game was played (e.g., "January 2024")
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string GameMonth { get; set; } = string.Empty;

        /// <summary>
        /// Year when the game was played
        /// </summary>
        public int GameYear { get; set; } = DateTime.UtcNow.Year;

        /// <summary>
        /// Current game turn (1, 2, 3, etc.)
        /// </summary>
        public int CurrentTurn { get; set; } = 1;

        /// <summary>
        /// Total number of turns in the game
        /// </summary>
        public int TotalTurns { get; set; } = 1;

        /// <summary>
        /// Game session ID if linked to an active session
        /// </summary>
        public Guid? GameSessionId { get; set; }

        [ForeignKey("GameSessionId")]
        public virtual GameSession? GameSession { get; set; }

        /// <summary>
        /// Map configuration ID used for this game
        /// </summary>
        public Guid? MapConfigurationId { get; set; }

        /// <summary>
        /// Complete game state as JSON - includes all tokens, positions, states
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string GameStateJson { get; set; } = "{}";

        /// <summary>
        /// Map overlays (regions, sectors, markers) as JSON
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string MapOverlaysJson { get; set; } = "{}";

        /// <summary>
        /// All forces (tokens) with positions and states as JSON
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string ForcesJson { get; set; } = "{}";

        /// <summary>
        /// All attack orders and details as JSON
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string AttacksJson { get; set; } = "{}";

        /// <summary>
        /// All maneuvers and movements as JSON
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string ManeuversJson { get; set; } = "{}";

        /// <summary>
        /// Defense elements (kill zones, minefields, obstacles) as JSON
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string DefenseElementsJson { get; set; } = "{}";

        /// <summary>
        /// Game turn snapshots - stores state at each turn
        /// Structure: { "turn1": {...}, "turn2": {...}, ... }
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string GameTurnsJson { get; set; } = "{}";

        /// <summary>
        /// Adjudication results and analysis as JSON
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string AdjudicationResultsJson { get; set; } = "{}";

        /// <summary>
        /// Combat simulation results as JSON
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string CombatResultsJson { get; set; } = "{}";

        /// <summary>
        /// Game status: "InProgress", "Completed", "Archived"
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "InProgress";

        /// <summary>
        /// User ID who saved/archived this game
        /// </summary>
        [MaxLength(255)]
        public string SavedByUserId { get; set; } = string.Empty;

        /// <summary>
        /// User name who saved/archived this game
        /// </summary>
        [MaxLength(255)]
        public string SavedByUserName { get; set; } = string.Empty;

        /// <summary>
        /// Date when game was saved/archived
        /// </summary>
        public DateTime? SavedDate { get; set; }

        /// <summary>
        /// Optional PDF document path for the archived game
        /// </summary>
        [MaxLength(500)]
        public string? PdfDocumentPath { get; set; }

        /// <summary>
        /// Additional metadata as JSON
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? MetadataJson { get; set; }
    }
}

