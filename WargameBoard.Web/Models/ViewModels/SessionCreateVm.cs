using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WargameBoard.Web.Models.ViewModels
{
    public class SessionCreateVm
    {
        [Required]
        [Display(Name = "Scenario")]
        public int ScenarioId { get; set; }

        [StringLength(500)]
        [Display(Name = "Session Notes")]
        public string? Notes { get; set; }

        // Select Lists
        public SelectList? Scenarios { get; set; }
    }
}
