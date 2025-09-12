using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    /// <summary>
    /// Game Session - Tracks active and completed games
    /// Tokens are bound to entities during active games, freed when games end
    /// </summary>
    [Table("GameSessions")]
    public class GameSession : BaseEntity
    {
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

    }

  
}
