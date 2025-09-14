using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WargameBoard.Web.Models.ViewModels
{
    public class TokenPieceEditVm
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Token Design")]
        public int TokenDesignId { get; set; }

        [Display(Name = "Side")]
        public int? SideId { get; set; }

        [StringLength(50)]
        [Display(Name = "Serial Number")]
        public string? Serial { get; set; }

        [StringLength(100)]
        [Display(Name = "Hardware Identity")]
        public string? HardwareIdentity { get; set; }

        [Display(Name = "Is Unique")]
        public bool IsUnique { get; set; } = false;

        // Select Lists
        public SelectList? TokenDesigns { get; set; }
        public SelectList? Sides { get; set; }
    }
}
