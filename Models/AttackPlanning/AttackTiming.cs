using System.ComponentModel.DataAnnotations;

namespace TechWebSol.Models.AttackPlanning
{
    /// <summary>
    /// Represents timing and coordination parameters for an attack
    /// </summary>
    public class AttackTiming : BaseEntity
    {
        [Required]
        [Range(1, 10, ErrorMessage = "Start turn must be between 1 and 10")]
        [Display(Name = "Start Turn")]
        public int StartTurn { get; set; } = 1;

        [Required]
        [Range(1, 5, ErrorMessage = "Duration must be between 1 and 5 turns")]
        [Display(Name = "Duration (Turns)")]
        public int DurationTurns { get; set; } = 1;

        [Required]
        [Display(Name = "Posture")]
        public string Posture { get; set; } = "Advance"; // "Advance", "Fix", "Feint"

        [Display(Name = "Expected End Turn")]
        public int ExpectedEndTurn => StartTurn + DurationTurns - 1;

        [Display(Name = "Timing Notes")]
        [MaxLength(300)]
        public string? Notes { get; set; }

        // Static options for UI dropdowns
        public static readonly string[] Postures = { "Advance", "Fix", "Feint" };
        public static readonly int[] AvailableStartTurns = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        public static readonly int[] AvailableDurations = { 1, 2, 3, 4, 5 };
    }
}
