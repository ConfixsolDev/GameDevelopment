using System.ComponentModel.DataAnnotations;
using WargameBoard.Core.Entities;

namespace WargameBoard.Web.Models.ViewModels
{
    public class ScenarioEditVm
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Scenario Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, 480)]
        [Display(Name = "Turn Length (Minutes)")]
        public int TurnLengthMinutes { get; set; } = 30;

        [Required]
        [Range(1, 100)]
        [Display(Name = "Maximum Turns")]
        public int MaxTurns { get; set; } = 20;

        [StringLength(50)]
        [Display(Name = "Weather Conditions")]
        public WeatherType Weather { get; set; }

        [StringLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}
