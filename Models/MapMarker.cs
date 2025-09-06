using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    [Table("MapMarkers")]
    public class MapMarker
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]  // we supply this key (e.g. "token_1757013297849")
        [MaxLength(100)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [ForeignKey("Token")]
        public long TokenId { get; set; }  // matches Token.Id type (long)

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Location { get; set; } = string.Empty;  // JSON {lat, lng}

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        [MaxLength(100)]
        public string TokenName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastUpdated { get; set; }

        public virtual Token Token { get; set; } = null!;
    }
}
