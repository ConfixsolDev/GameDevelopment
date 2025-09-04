using Microsoft.AspNetCore.Identity;

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
        public string Access { get; set; } = string.Empty;
    }
}
