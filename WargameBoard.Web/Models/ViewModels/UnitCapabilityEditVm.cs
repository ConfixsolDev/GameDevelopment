using System.ComponentModel.DataAnnotations;

namespace WargameBoard.Web.Models.ViewModels
{
    public class UnitCapabilityEditVm
    {
        [Required]
        [Display(Name = "Unit")]
        public int UnitId { get; set; }

        [Required]
        [Range(0, 20)]
        [Display(Name = "Attack vs Soft Targets")]
        public int AttackSoft { get; set; }

        [Required]
        [Range(0, 20)]
        [Display(Name = "Attack vs Hard Targets")]
        public int AttackHard { get; set; }

        [Required]
        [Range(0, 20)]
        [Display(Name = "Defense Rating")]
        public int Defense { get; set; }

        [Required]
        [Range(0, 20)]
        [Display(Name = "Indirect Support")]
        public int IndirectSupport { get; set; }

        [Range(0, 10)]
        [Display(Name = "ATGM Count")]
        public int? AtgmCount { get; set; }

        [Range(0, 10)]
        [Display(Name = "Mortars Count")]
        public int? MortarsCount { get; set; }

        [Range(0, 10)]
        [Display(Name = "Rockets Count")]
        public int? RocketsCount { get; set; }

        [Range(0, 10)]
        [Display(Name = "HMG Count")]
        public int? HmgCount { get; set; }
    }
}
