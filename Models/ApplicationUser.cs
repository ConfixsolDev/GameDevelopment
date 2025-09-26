using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(255)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(255)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(255)]
        public string Designation { get; set; } = string.Empty;

        [StringLength(255)]
        public string Department { get; set; } = string.Empty;

        [StringLength(255)]
        public string FullName => $"{this.FirstName} {this.LastName}".Trim();

        public bool IsOnline { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginDate { get; set; }
        public int LoginCount { get; set; }

        [ScaffoldColumn(false)]
        [DisplayName("User Code")]
        public string UserCode { get; set; } = string.Empty;

        [MaxLength(128)]
        [NotMapped]
        public string RoleName { get; set; } = string.Empty;

        [ScaffoldColumn(false)]
        [StringLength(255)]
        public string CreatedBy { get; set; } = string.Empty;

        [ScaffoldColumn(false)]
        [StringLength(255)]
        public string ModifiedBy { get; set; } = string.Empty;

        [ScaffoldColumn(false)]
        [DisplayName("Updated Date")]
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        [ScaffoldColumn(false)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ScaffoldColumn(false)]
        [DisplayName("Team")]
        public string TeamCode { get; set; } = string.Empty;

        [ScaffoldColumn(false)]
        [DisplayName("Sub-Team")]
        public string SubTeamCode { get; set; } = string.Empty;
        public DateTime? AssignDate { get; set; }

        // Foreign key to Team
        public Guid? TeamId { get; set; }
        public string? TeamTypeCode { get; set; }

        // Navigation property
        public virtual Team? Team { get; set; }

        [MaxLength(250)]
        public string HomeUrl { get; set; } = string.Empty;

        [NotMapped]
        public bool isSuperAdmin { get; set; } = false;

        public bool IsDeleted { get; set; } = false;
    }
}

