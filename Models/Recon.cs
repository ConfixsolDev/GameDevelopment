using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    [Table("Recon")]
    public class Recon : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        [DisplayName("Reconnaissance Type")]
        public string ReconType { get; set; }

        [MaxLength(200)]
        [DisplayName("Location")]
        public string Location { get; set; }

        [MaxLength(20)]
        [DisplayName("Confidence Level")]
        public string Confidence { get; set; } // High, Medium, Low

        [MaxLength(500)]
        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Optional: Link to map token
        [DisplayName("Token ID")]
        public Guid? TokenId { get; set; }

        [ForeignKey("TokenId")]
        public virtual Token Token { get; set; }
    }
}
