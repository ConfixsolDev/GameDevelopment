using System.ComponentModel.DataAnnotations;

namespace TechWebSol.Models
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(255)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string UpdatedBy { get; set; } = string.Empty;

        public DateTime? UpdatedDate { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;
    }
}
