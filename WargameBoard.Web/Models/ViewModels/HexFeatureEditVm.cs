using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WargameBoard.Core.Entities;

namespace WargameBoard.Web.Models.ViewModels
{
    public class HexFeatureEditVm
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Hex")]
        public int HexId { get; set; }

        [Required]
        [Display(Name = "Feature Kind")]
        public FeatureKind? FeatureKind { get; set; }
        public int? FeatureKindId { get; set; }

        [Display(Name = "Fortification Type")]
        public int? FortificationTypeId { get; set; }

        [Display(Name = "Obstacle Type")]
        public int? ObstacleTypeId { get; set; }

        [Display(Name = "Side")]
        public int? SideId { get; set; }

        // Select Lists
        public SelectList? FortificationTypes { get; set; }
        public SelectList? ObstacleTypes { get; set; }
        public SelectList? Sides { get; set; }
    }
}
