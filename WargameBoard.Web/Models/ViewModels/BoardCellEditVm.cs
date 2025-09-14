using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WargameBoard.Web.Models.ViewModels
{
    public class BoardCellEditVm
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Board")]
        public int BoardId { get; set; }

        [Required]
        [Range(0, 100)]
        [Display(Name = "Row")]
        public int Row { get; set; }

        [Required]
        [Range(0, 100)]
        [Display(Name = "Column")]
        public int Col { get; set; }

        [StringLength(50)]
        [Display(Name = "Sensor Address")]
        public string? SensorAddress { get; set; }

        [Display(Name = "Hex")]
        public int? HexId { get; set; }

        [Range(0, 100)]
        [Display(Name = "Threshold")]
        public int? Threshold { get; set; }

        // Select Lists
        public SelectList? Boards { get; set; }
        public SelectList? Hexes { get; set; }
    }
}
