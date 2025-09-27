using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    [Table("MapRegions")]
    public class MapRegion : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Geometry { get; set; } = string.Empty; // GeoJSON geometry

        [Column(TypeName = "nvarchar(max)")]
        public string? Properties { get; set; } // Additional properties as JSON

        [Column(TypeName = "decimal(18,6)")]
        public decimal? AreaM2 { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal? CenterLat { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal? CenterLng { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsLocked { get; set; } = false;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? RegionType { get; set; } // 'main', 'sector', etc.

        public DateTime? LastModified { get; set; }
    }
}

