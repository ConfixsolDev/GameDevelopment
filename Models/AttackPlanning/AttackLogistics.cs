using System.ComponentModel.DataAnnotations;

namespace TechWebSol.Models.AttackPlanning
{
    /// <summary>
    /// Represents logistics and supply considerations for an attack
    /// </summary>
    public class AttackLogistics : BaseEntity
    {
        [Required]
        [Display(Name = "Supply Threshold")]
        public string SupplyThreshold { get; set; } = "Green"; // "Green", "Amber", "Red"

        [Display(Name = "Minimum Supply Required (%)")]
        [Range(0, 100, ErrorMessage = "Supply percentage must be between 0 and 100")]
        public int MinimumSupplyRequired { get; set; } = 50;

        [Display(Name = "Expected Supply Consumption (%)")]
        [Range(0, 100, ErrorMessage = "Supply consumption must be between 0 and 100")]
        public int ExpectedSupplyConsumption { get; set; } = 20;

        [Display(Name = "Resupply Available")]
        public bool ResupplyAvailable { get; set; } = false;

        [Display(Name = "Resupply Turn")]
        [Range(1, 10, ErrorMessage = "Resupply turn must be between 1 and 10")]
        public int? ResupplyTurn { get; set; }

        [Display(Name = "Logistics Notes")]
        [MaxLength(300)]
        public string? Notes { get; set; }

        // Calculated properties
        public int RemainingSupplyAfterAttack => MinimumSupplyRequired - ExpectedSupplyConsumption;
        public bool IsSupplySufficient => RemainingSupplyAfterAttack >= 20; // Minimum 20% required
        public string SupplyRiskLevel => RemainingSupplyAfterAttack switch
        {
            >= 50 => "Low",
            >= 30 => "Medium",
            >= 20 => "High",
            _ => "Critical"
        };

        // Static options for UI dropdowns
        public static readonly string[] SupplyThresholds = { "Green", "Amber", "Red" };
        public static readonly int[] AvailableResupplyTurns = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    }
}
