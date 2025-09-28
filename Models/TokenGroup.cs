using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    /// <summary>
    /// Token Group - Administrator-managed groups for organizing tokens
    /// Examples: "Company A", "Brigade 1", "Department Alpha", etc.
    /// </summary>
    [Table("TokenGroups")]
    public class TokenGroup : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // e.g., "Company A", "Brigade 1"

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string GroupCode { get; set; } = string.Empty; // e.g., "COMP_A", "BRIG_1"

        [MaxLength(50)]
        public string? Category { get; set; } // e.g., "Company", "Brigade", "Department"

        // Navigation properties
        public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();
        public virtual ICollection<TeamTokenGroupAssignment> TeamAssignments { get; set; } = new List<TeamTokenGroupAssignment>();
    }

    /// <summary>
    /// Assignment of token groups to teams
    /// Allows administrators to assign specific token groups to specific teams
    /// </summary>
    [Table("TeamTokenGroupAssignments")]
    public class TeamTokenGroupAssignment : BaseEntity
    {
        [Required]
        public Guid TokenGroupId { get; set; } // Foreign key to TokenGroup.Id
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Additional assignment-specific fields
        [MaxLength(50)]
        public string? AssignedByUserId { get; set; }

        [MaxLength(50)]
        public string? AssignedByUserName { get; set; }

        // Navigation properties
        public virtual Team Team { get; set; } = null!;
        public virtual TokenGroup TokenGroup { get; set; } = null!;
    }
}
