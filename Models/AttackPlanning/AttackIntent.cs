using System.ComponentModel.DataAnnotations;

namespace TechWebSol.Models.AttackPlanning
{
    /// <summary>
    /// Represents the tactical intent and approach for an attack
    /// </summary>
    public class AttackIntent:BaseEntity
    {
        [Required]
        [Display(Name = "Attack Preparation")]
        public string AttackPreparation { get; set; } = "Deliberate"; // "Hasty", "Deliberate"

        [Required]
        [Display(Name = "NATO Attack Type")]
        public string NatoAttackType { get; set; } = "frontal"; // "frontal", "flanking", "envelopment", "penetration", "raid", "ambush"

        [Required]
        [Display(Name = "Attack Intensity")]
        public string AttackIntensity { get; set; } = "standard"; // "light", "standard", "heavy", "overwhelming"

        [Required]
        [Display(Name = "Coordination Type")]
        public string CoordinationType { get; set; } = "independent"; // "independent", "supporting", "main", "feint", "exploitation"

        [Required]
        [Display(Name = "Desired Effect")]
        public string DesiredEffect { get; set; } = "Destroy"; // "Destroy", "Disrupt", "Fix", "Delay", "Deny"

        [Display(Name = "Intent Notes")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Static options for UI dropdowns
        public static readonly string[] AttackPreparations = { "Hasty", "Deliberate" };
        public static readonly string[] NatoAttackTypes = { "frontal", "flanking", "envelopment", "penetration", "raid", "ambush" };
        public static readonly string[] AttackIntensities = { "light", "standard", "heavy", "overwhelming" };
        public static readonly string[] CoordinationTypes = { "independent", "supporting", "main", "feint", "exploitation" };
        public static readonly string[] DesiredEffects = { "Destroy", "Disrupt", "Fix", "Delay", "Deny" };

        // NATO Attack Type Properties
        public string GetNatoAttackTypeName()
        {
            return NatoAttackType switch
            {
                "frontal" => "Frontal Attack",
                "flanking" => "Flanking Attack",
                "envelopment" => "Envelopment",
                "penetration" => "Penetration",
                "raid" => "Raid",
                "ambush" => "Ambush",
                _ => "Unknown Attack Type"
            };
        }

        public string GetAttackIntensityName()
        {
            return AttackIntensity switch
            {
                "light" => "Light Attack",
                "standard" => "Standard Attack",
                "heavy" => "Heavy Attack",
                "overwhelming" => "Overwhelming Attack",
                _ => "Unknown Intensity"
            };
        }

        public string GetCoordinationTypeName()
        {
            return CoordinationType switch
            {
                "independent" => "Independent Action",
                "supporting" => "Supporting Attack",
                "main" => "Main Effort",
                "feint" => "Feint/Diversion",
                "exploitation" => "Exploitation",
                _ => "Unknown Coordination"
            };
        }

        public string GetNatoSymbol()
        {
            return NatoAttackType switch
            {
                "frontal" => "⇨",
                "flanking" => "↗",
                "envelopment" => "↻",
                "penetration" => "⇉",
                "raid" => "⚔",
                "ambush" => "⚡",
                _ => "→"
            };
        }

        public string GetIntensityColor()
        {
            return AttackIntensity switch
            {
                "light" => "#ffaa44",
                "standard" => "#ff4444",
                "heavy" => "#cc0000",
                "overwhelming" => "#990000",
                _ => "#ff4444"
            };
        }

        public int GetIntensityWeight()
        {
            return AttackIntensity switch
            {
                "light" => 2,
                "standard" => 4,
                "heavy" => 6,
                "overwhelming" => 8,
                _ => 4
            };
        }
    }
}
