using System.ComponentModel.DataAnnotations;

namespace TechWebSol.Models
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(255)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        
        [StringLength(255)]
        public string UpdatedBy { get; set; } = string.Empty;

        public Guid? TeamId { get; set; } 

        public DateTime? UpdatedDate { get; set; } = DateTime.Now;
        
        public bool IsActive { get; set; } = true;
    }
  
    public class BaseEntityDataEntry:BaseEntity
    {
        public Guid? TokenId { get; set; }
        public Guid? BrigadeId { get; set; }
    }
}
