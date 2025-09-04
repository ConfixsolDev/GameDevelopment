using System.ComponentModel.DataAnnotations;

namespace TechWebSol.Models.DocumentModal
{
    public class FileUploadVM
    {
        [Required]
        [Display(Name = "File")]
        public List<IFormFile> FormFiles { get; set; } = new List<IFormFile>();
        public Guid? Id { get; set; }
        public string SuccessMessage { get; set; }
        public string Path { get; set; }
        [Display(Name = "Order")]
        public int PriorityOrder { get; set; } = 0;
        public Guid EntityId { get; set; }
        [Display(Name = "Title for upload Document")]
        public string FileTitle { get; set; }
    }

    public class FileUploadVMEdit
    {
        [Display(Name = "File")]
        public List<IFormFile> FormFiles { get; set; } = new List<IFormFile>();
        public Guid? Id { get; set; }
        public string SuccessMessage { get; set; }
        public string Path { get; set; }
        [Display(Name = "Order")]
        public int PriorityOrder { get; set; } = 0;
        public Guid EntityId { get; set; }
        [Display(Name = "Title")]
        public string FileTitle { get; set; }
    }
}
