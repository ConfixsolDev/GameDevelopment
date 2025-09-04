using System.ComponentModel.DataAnnotations;

namespace TechWebSol.Models.DocumentModal
{
    public class DocumentEntity : BaseEntity
    {
        public Guid ForiegnEntityId{ get; set; }
        
        [MaxLength(250)]
        public string FileName { get; set; }
     
        [MaxLength(100)]
        public string FileType { get; set; }

        public int PriorityOrder { get; set; } = 0;
        public string FileURL { get; set; }
        [MaxLength(256)]
        public string FileTitle { get; set; }
    }
}