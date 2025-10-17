using System.ComponentModel.DataAnnotations;

namespace TechWebSol.Models.AttackPlanning
{
    /// <summary>
    /// Represents artillery and engineer support for an attack
    /// </summary>
    public class FiresSupport : BaseEntity
    {
        [Display(Name = "Artillery Units Attached")]
        public List<string> ArtilleryAttached { get; set; } = new List<string>(); // Unit IDs

        [Required]
        [Display(Name = "Artillery Task")]
        public string ArtilleryTask { get; set; } = "PrepFires"; // "PrepFires", "OnCall", "Counterbattery"

        [Display(Name = "Engineers Present")]
        public bool EngineersPresent { get; set; } = false;

        [Display(Name = "Engineer Units")]
        public List<string> EngineerUnits { get; set; } = new List<string>(); // Unit IDs

        [Display(Name = "Support Notes")]
        [MaxLength(300)]
        public string? Notes { get; set; }

        // Calculated properties
        public int TotalArtilleryUnits => ArtilleryAttached?.Count ?? 0;
        public int TotalEngineerUnits => EngineerUnits?.Count ?? 0;
        public bool HasSupport => TotalArtilleryUnits > 0 || EngineersPresent;

        // Static options for UI dropdowns
        public static readonly string[] ArtilleryTasks = { "PrepFires", "OnCall", "Counterbattery" };
    }
}
