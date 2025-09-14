using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WargameBoard.Web.Models.ViewModels
{
    public class TokenDesignEditVm
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Token Type")]
        public int TokenGroupId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Token Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Default Side")]
        public int? DefaultSideId { get; set; }

        [Required]
        [Range(1, 100)]
        [Display(Name = "Width (mm)")]
        public decimal WidthMm { get; set; } = 25;

        [Required]
        [Range(1, 100)]
        [Display(Name = "Height (mm)")]
        public decimal HeightMm { get; set; } = 25;

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Select Lists
        public SelectList? TokenGroups { get; set; }
        public SelectList? Sides { get; set; }
    }
}
