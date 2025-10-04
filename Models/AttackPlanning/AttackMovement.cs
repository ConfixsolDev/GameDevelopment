using System.ComponentModel.DataAnnotations;

namespace TechWebSol.Models.AttackPlanning
{
    /// <summary>
    /// Represents movement and positioning parameters for an attack
    /// </summary>
    public class AttackMovement : BaseEntity
    {
        [Required]
        [Range(0.0, 0.5, ErrorMessage = "MP Reserve must be between 0% and 50%")]
        [Display(Name = "Movement Points Reserve (%)")]
        public decimal MpReservePercent { get; set; } = 0.1m; // 10% default

        [Display(Name = "Waypoints")]
        public List<Waypoint> Waypoints { get; set; } = new List<Waypoint>();

        [Display(Name = "Movement Notes")]
        [MaxLength(300)]
        public string? Notes { get; set; }

        // Calculated properties
        public int TotalWaypoints => Waypoints?.Count ?? 0;
        public decimal TotalDistance => Waypoints?.Sum(w => w.DistanceToNext) ?? 0;
        public decimal EstimatedMovementCost => TotalDistance * (1 - MpReservePercent);
    }

    /// <summary>
    /// Represents a waypoint in the attack movement plan
    /// </summary>
    public class Waypoint : BaseEntity
    {
        [Required]
        [Display(Name = "Latitude")]
        public double Latitude { get; set; }

        [Required]
        [Display(Name = "Longitude")]
        public double Longitude { get; set; }

        [Display(Name = "Waypoint Name")]
        [MaxLength(50)]
        public string? Name { get; set; }

        [Display(Name = "Distance to Next (km)")]
        public decimal DistanceToNext { get; set; } = 0;

        [Display(Name = "Notes")]
        [MaxLength(100)]
        public string? Notes { get; set; }
    }
}
