using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WargameBoard.Core.Entities;

namespace WargameBoard.Web.Models.ViewModels
{
    public class ScenarioUnitEditVm
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Scenario")]
        public int ScenarioId { get; set; }

        [Required]
        [Display(Name = "Unit")]
        public int UnitId { get; set; }

        [Required]
        [Display(Name = "Starting Hex")]
        public int StartHexId { get; set; }

        [Required]
        [Display(Name = "Steps")]
        public int Steps { get; set; } = 1;

        [Required]
        [Display(Name = "Posture")]
        public int Posture { get; set; }

        [Display(Name = "Hidden")]
        public bool Hidden { get; set; } = false;

        // Select Lists
        public SelectList? Scenarios { get; set; }
        public SelectList? Units { get; set; }
        public SelectList? Hexes { get; set; }
    }
}
