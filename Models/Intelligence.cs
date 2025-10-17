using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    [Table("Intelligence")]
    public class Intelligence : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        [DisplayName("Report Title")]
        public string Title { get; set; }

        [MaxLength(500)]
        [DisplayName("Description")]
        public string Description { get; set; }

        [MaxLength(100)]
        [DisplayName("Source")]
        public string Source { get; set; }

        [MaxLength(20)]
        [DisplayName("Priority")]
        public string Priority { get; set; } // High, Medium, Low

        [DisplayName("Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Optional: Link to map token
        [DisplayName("Token ID")]
        public Guid? TokenId { get; set; }

        [ForeignKey("TokenId")]
        public virtual Token Token { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }
    }
}
