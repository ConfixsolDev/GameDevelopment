using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    /// <summary>
    /// Game Session - Tracks active and completed games
    /// Tokens are bound to entities during active games, freed when games end
    /// </summary>
    [Table("GameSessions")]
    public class GameSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // e.g., "War Game 2024", "Training Exercise Alpha"

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string SessionCode { get; set; } = string.Empty; // e.g., "WG2024", "TEA001"

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // "Active", "Completed", "Cancelled"

        [Required]
        [MaxLength(50)]
        public string CreatedByUserId { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? CreatedByUserName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<TokenBinding> TokenBindings { get; set; } = new List<TokenBinding>();
    }

    /// <summary>
    /// Token Binding - Temporary binding of tokens to entities during a game session
    /// When game ends, bindings are cleared but tokens remain for reuse
    /// </summary>
    [Table("TokenBindings")]
    public class TokenBinding
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GameSessionId { get; set; }

        [Required]
        public int TokenGroupId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TeamId { get; set; } = string.Empty; // TeamCode + SubTeamCode

        [Required]
        [MaxLength(100)]
        public string EntityName { get; set; } = string.Empty; // e.g., "Company A", "Brigade 1"

        [MaxLength(50)]
        public string? EntityCode { get; set; } // e.g., "COMP_A", "BRIG_1"

        [MaxLength(500)]
        public string? EntityDescription { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime BoundAt { get; set; } = DateTime.UtcNow;

        public DateTime? UnboundAt { get; set; }

        [Required]
        [MaxLength(50)]
        public string BoundByUserId { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? BoundByUserName { get; set; }

        // Navigation properties
        public virtual GameSession GameSession { get; set; } = null!;
        public virtual TokenGroup TokenGroup { get; set; } = null!;
    }

    /// <summary>
    /// Free Token - Tokens that are not currently bound to any game session
    /// These can be assigned to new entities in new games
    /// </summary>
    [Table("FreeTokens")]
    public class FreeToken
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        public int TouchCount { get; set; }

        [MaxLength(20)]
        public string System { get; set; } = string.Empty; // "simplified" or "complex"

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUsed { get; set; }

        public int UsageCount { get; set; } = 0;

        [Required]
        [MaxLength(50)]
        public string CreatedByUserId { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? CreatedByUserName { get; set; }

        // Token signature data (simplified)
        [MaxLength(1000)]
        public string? Distances { get; set; } // JSON

        [MaxLength(1000)]
        public string? Angles { get; set; } // JSON

        [MaxLength(100)]
        public string? Center { get; set; } // JSON

        // Token signature data (complex)
        [MaxLength(2000)]
        public string? ComplexSignature { get; set; } // JSON for complex tokens
    }
}
