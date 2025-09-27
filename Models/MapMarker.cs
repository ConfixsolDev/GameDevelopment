using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    [Table("MapMarkers")]
    public class MapMarker:BaseEntity
    {
        [Required]
        [ForeignKey("Token")]
        public Guid? TokenId { get; set; }  

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Location { get; set; } = string.Empty; 

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        [MaxLength(100)]
        public string TokenName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime? LastUpdated { get; set; }

        public virtual Token Token { get; set; } = null!;
    }
}
