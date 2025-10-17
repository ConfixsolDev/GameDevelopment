using System.ComponentModel.DataAnnotations;

namespace TechWebSol.Models.AttackPlanning
{
    /// <summary>
    /// Represents fog of war and uncertainty management for an attack
    /// </summary>
    public class FogOfWar : BaseEntity
    {
        [Required]
        [Range(0.0, 1.0, ErrorMessage = "Detection confidence must be between 0% and 100%")]
        [Display(Name = "Detection Confidence")]
        public decimal DetectionConfidence { get; set; } = 0.72m; // 72% default

        [Display(Name = "Commit with Uncertainty")]
        public bool CommitWithUncertainty { get; set; } = true;

        [Display(Name = "Abort Criteria")]
        public List<AbortCriteria> AbortCriteria { get; set; } = new List<AbortCriteria>();

        [Display(Name = "Fog of War Notes")]
        [MaxLength(300)]
        public string? Notes { get; set; }

        // Calculated properties
        public string ConfidenceLevel => DetectionConfidence switch
        {
            >= 0.8m => "High",
            >= 0.6m => "Medium",
            >= 0.4m => "Low",
            _ => "Very Low"
        };

        public bool IsHighRisk => DetectionConfidence < 0.5m;
        public int TotalAbortCriteria => AbortCriteria?.Count ?? 0;
    }

    /// <summary>
    /// Represents criteria for aborting an attack due to changing conditions
    /// </summary>
    public class AbortCriteria:BaseEntity
    {
        [Required]
        [Display(Name = "Criteria Type")]
        public string Type { get; set; } = "DetectionBelow"; // "DetectionBelow", "SupplyBelow", "CasualtyAbove"

        [Required]
        [Display(Name = "Threshold Value")]
        public string Value { get; set; } = "0.4"; // Threshold value

        [Display(Name = "Criteria Notes")]
        [MaxLength(100)]
        public string? Notes { get; set; }

        // Static options for UI dropdowns
        public static readonly string[] CriteriaTypes = { "DetectionBelow", "SupplyBelow", "CasualtyAbove", "TimeExceeded" };
    }
}
