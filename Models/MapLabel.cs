using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    [Table("MapLabels")]
    public class MapLabel : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Text { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,6)")]
        public decimal Latitude { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,6)")]
        public decimal Longitude { get; set; }

        [MaxLength(50)]
        public string? LabelType { get; set; } // 'point', 'area', etc.

        [MaxLength(50)]
        public string? Color { get; set; } = "#000000";

        [MaxLength(50)]
        public string? Icon { get; set; }

        public int? FontSize { get; set; } = 12;

        [MaxLength(50)]
        public string? FontWeight { get; set; } = "normal";

        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Properties { get; set; } // Additional properties as JSON

        public DateTime? LastModified { get; set; }
    }
}

