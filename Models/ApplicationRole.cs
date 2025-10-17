using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base()
        {
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
        }

        // Custom properties that the existing code expects
        public string ApplicationId { get; set; } = string.Empty;
        [Column(TypeName = "nvarchar(max)")]
        public string Access { get; set; } = string.Empty;
    }
}
