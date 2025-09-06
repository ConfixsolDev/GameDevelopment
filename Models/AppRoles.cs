using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    public class AppRoles
    {
        public string Id { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;
        [Column(TypeName = "nvarchar(max)")]
        public string RoleAccess { get; set; } = string.Empty;
    }
}
