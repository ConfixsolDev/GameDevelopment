using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WargameBoard.Web.Models.ViewModels
{
    public class UnitEditVm
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Unit Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Side")]
        public int SideId { get; set; }

        [Required]
        [Display(Name = "Unit Type")]
        public int UnitTypeId { get; set; }

        [Required]
        [Range(1, 1000)]
        [Display(Name = "Personnel Count")]
        public int? Personnel { get; set; }

        [Required]
        [Range(0, 100)]
        [Display(Name = "Primary Vehicles")]
        public int? VehiclesPrimary { get; set; }

        [Required]
        [Range(1, 10)]
        [Display(Name = "Quality Rating")]
        public int Quality { get; set; }

        [Required]
        [Range(1, 10)]
        [Display(Name = "Cohesion Rating")]
        public int Cohesion { get; set; }

        [Required]
        [Display(Name = "Movement Profile")]
        public int? MovementProfileId { get; set; }

        // Select Lists
        public SelectList? Sides { get; set; }
        public SelectList? UnitTypes { get; set; }
        public SelectList? MovementProfiles { get; set; }
    }
}
