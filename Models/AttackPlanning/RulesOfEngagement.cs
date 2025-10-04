using System.ComponentModel.DataAnnotations;

namespace TechWebSol.Models.AttackPlanning
{
    /// <summary>
    /// Represents rules of engagement and collateral damage considerations
    /// </summary>
    public class RulesOfEngagement : BaseEntity
    {
        [Required]
        [Display(Name = "Collateral Sensitivity")]
        public string CollateralSensitivity { get; set; } = "Medium"; // "Low", "Medium", "High"

        [Display(Name = "Civilian Population Present")]
        public bool CivilianPopulationPresent { get; set; } = false;

        [Display(Name = "Civilian Population Density")]
        [Range(0, 100, ErrorMessage = "Population density must be between 0 and 100")]
        public int CivilianPopulationDensity { get; set; } = 0; // 0-100%

        [Display(Name = "Infrastructure Present")]
        public bool InfrastructurePresent { get; set; } = false;

        [Display(Name = "Infrastructure Types")]
        public List<string> InfrastructureTypes { get; set; } = new List<string>(); // "Hospitals", "Schools", "Religious", "Critical"

        [Display(Name = "Weapon Restrictions")]
        public List<string> WeaponRestrictions { get; set; } = new List<string>(); // "Heavy Artillery", "Air Support", "Incendiary"

        [Display(Name = "ROE Notes")]
        [MaxLength(300)]
        public string? Notes { get; set; }

        // Calculated properties
        public string RiskLevel => CollateralSensitivity switch
        {
            "High" => "High Risk",
            "Medium" => "Medium Risk",
            "Low" => "Low Risk",
            _ => "Unknown Risk"
        };

        public bool HasRestrictions => WeaponRestrictions?.Any() ?? false;
        public bool RequiresApproval => CollateralSensitivity == "High" || CivilianPopulationDensity > 50;

        // Static options for UI dropdowns
        public static readonly string[] CollateralSensitivities = { "Low", "Medium", "High" };
        public static readonly string[] InfrastructureTypesOptions = { "Hospitals", "Schools", "Religious", "Critical", "Residential", "Commercial" };
        public static readonly string[] WeaponRestrictionsOptions = { "Heavy Artillery", "Air Support", "Incendiary", "Cluster Munitions", "Precision Guided" };
    }
}
