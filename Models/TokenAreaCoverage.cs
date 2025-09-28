using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    [Table("TokenAreaCoverages")]
    public class TokenAreaCoverage : BaseEntity
    {
        [Required]
        [ForeignKey("Token")]
        public Guid TokenId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // "Operational Area", "Surveillance Zone", "Coverage Area", etc.

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Geometry { get; set; } = string.Empty; // GeoJSON polygon/circle

        [Column(TypeName = "decimal(18,6)")]
        public decimal? AreaKm2 { get; set; } // Calculated area

        [Column(TypeName = "decimal(8,2)")]
        public decimal? RadiusKm { get; set; } // Radius for circular areas

        [MaxLength(50)]
        public string CoverageType { get; set; } = "Operational"; 
        // "Operational", "Surveillance", "Defensive", "Support", "Patrol", "Combat", "Reconnaissance"

        [MaxLength(50)]
        public string ShapeType { get; set; } = "Circle"; // "Circle", "Polygon", "Rectangle"

        public bool IsDynamic { get; set; } = true; // Updates when token moves

        public DateTime? LastUpdated { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation properties
        public virtual Token Token { get; set; } = null!;
    }
}
