using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    /// <summary>
    /// Team model for organizing users into teams
    /// </summary>
    [Table("Teams")]
    public class Team : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // e.g., "Alpha Team", "Bravo Team"

        [Required]
        [MaxLength(50)]
        public string TeamCode { get; set; } = string.Empty; // e.g., "ALPHA", "BRAVO"

        [MaxLength(50)]
        public string? SubTeamCode { get; set; } // e.g., "ALPHA_1", "ALPHA_2"

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; } // e.g., "Company", "Brigade", "Department"

        public bool IsActive { get; set; } = true;

        // Additional team-specific fields
        [MaxLength(50)]
        public string? CreatedByUserId { get; set; }

        [MaxLength(50)]
        public string? CreatedByUserName { get; set; }

        // Navigation properties
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();
        public virtual ICollection<TeamTokenGroupAssignment> TokenGroupAssignments { get; set; } = new List<TeamTokenGroupAssignment>();
    }
}
