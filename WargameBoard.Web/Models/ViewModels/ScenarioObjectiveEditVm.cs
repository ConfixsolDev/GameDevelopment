using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WargameBoard.Core.Entities;

namespace WargameBoard.Web.Models.ViewModels
{
    public class ScenarioObjectiveEditVm
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Scenario")]
        public int ScenarioId { get; set; }

        [Required]
        [Display(Name = "Hex")]
        public int HexId { get; set; }

        [Required]
        [Display(Name = "Side")]
        public int SideId { get; set; }

        [Required]
        [Display(Name = "Victory Points")]
        public int VictoryPoints { get; set; } = 10;

        [Required]
        [Display(Name = "Condition Kind")]
        public int ConditionKind { get; set; } // <- enum

        [Display(Name = "Turn Threshold")]
        public int? TurnThreshold { get; set; }

        // Select Lists
        public SelectList? Scenarios { get; set; }
        public SelectList? Hexes { get; set; }
        public SelectList? Sides { get; set; }
    }
}
