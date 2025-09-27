using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    [Table("Recon")]
    public class Recon : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string ReconType { get; set; }

        [MaxLength(200)]
        public string Location { get; set; }

        [MaxLength(20)]
        public string Confidence { get; set; } // High, Medium, Low

        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Optional: Link to map token
        public Guid? TokenId { get; set; }

        [ForeignKey("TokenId")]
        public virtual Token Token { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }
    }
}
