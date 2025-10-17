using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    /// <summary>
    /// Represents a defensive element (kill zone, minefield, obstacle, etc.) in the wargame
    /// Associated with military tokens to assign responsibility
    /// </summary>
    public class DefenseElement : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public Guid ElementId { get; set; } // Unique identifier from client

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // killzone, minefield, obstacle, position, route, line

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // primary, secondary, engagement, antipersonnel, etc.

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Coordinates { get; set; } = string.Empty; // JSON array of coordinates

        /// <summary>
        /// Associated token ID - assigns responsibility for this defense element
        /// </summary>
        public Guid? TokenId { get; set; }

        [ForeignKey("TokenId")]
        public virtual Token? Token { get; set; }

        /// <summary>
        /// Team navigation property (TeamId inherited from BaseEntity)
        /// </summary>
        [ForeignKey("TeamId")]
        public virtual Team? Team { get; set; }

        /// <summary>
        /// Strength/effectiveness of this defense element (0-100)
        /// </summary>
        [Range(0, 100)]
        public int Strength { get; set; } = 100;

        /// <summary>
        /// Effectiveness multiplier (0.5 - 2.0)
        /// </summary>
        [Range(0.5, 2.0)]
        public double Effectiveness { get; set; } = 1.0;

        /// <summary>
        /// Visibility setting: friendly, control, force, all
        /// Controls which teams can see this element
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Visibility { get; set; } = "friendly";

        /// <summary>
        /// Game session this defense element belongs to
        /// </summary>
        public Guid? GameSessionId { get; set; }

        [ForeignKey("GameSessionId")]
        public virtual GameSession? GameSession { get; set; }

        /// <summary>
        /// User who created this defense element
        /// </summary>
        public Guid CreatedByUserId { get; set; }

        /// <summary>
        /// Status of the defense element: active, destroyed, inactive
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "active";

        /// <summary>
        /// Additional metadata as JSON
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? Metadata { get; set; }

        /// <summary>
        /// Notes about this defense element
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

