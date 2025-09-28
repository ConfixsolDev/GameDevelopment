using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    [Table("MapConfigurations")]
    public class MapConfiguration : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string ConfigurationType { get; set; } = string.Empty; // 'base_layer', 'panel_state', etc.

        [Required]
        [MaxLength(200)]
        public string Key { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string? Value { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Properties { get; set; } // Additional properties as JSON

        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime? LastModified { get; set; }
    }
}

