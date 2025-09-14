using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WargameBoard.Web.Models.ViewModels
{
    public class HexEditVm
    {
        public int Id { get; set; }

        [Required]
        [Range(-50, 50)]
        [Display(Name = "Q Coordinate")]
        public int Q { get; set; }

        [Required]
        [Range(-50, 50)]
        [Display(Name = "R Coordinate")]
        public int R { get; set; }

        [Required]
        [Display(Name = "Terrain Type")]
        public int TerrainTypeId { get; set; }

        [StringLength(200)]
        [Display(Name = "Key Feature")]
        public string? KeyFeature { get; set; }

        // Select Lists
        public SelectList? TerrainTypes { get; set; }
    }
}
