using System.ComponentModel.DataAnnotations;

namespace TechWebSol.Models.AttackPlanning
{
    /// <summary>
    /// Represents the tactical intent and approach for an attack
    /// </summary>
    public class AttackIntent:BaseEntity
    {
        [Required]
        [Display(Name = "Attack Type")]
        public string AttackType { get; set; } = "Deliberate"; // "Hasty", "Deliberate"

        [Required]
        [Display(Name = "Maneuver Form")]
        public string ManeuverForm { get; set; } = "Frontal"; // "Penetration", "Envelopment", "Frontal", "Turning"

        [Required]
        [Display(Name = "Desired Effect")]
        public string DesiredEffect { get; set; } = "Destroy"; // "Destroy", "Disrupt", "Fix", "Delay", "Deny"

        [Display(Name = "Intent Notes")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Static options for UI dropdowns
        public static readonly string[] AttackTypes = { "Hasty", "Deliberate" };
        public static readonly string[] ManeuverForms = { "Penetration", "Envelopment", "Frontal", "Turning" };
        public static readonly string[] DesiredEffects = { "Destroy", "Disrupt", "Fix", "Delay", "Deny" };
    }
}
