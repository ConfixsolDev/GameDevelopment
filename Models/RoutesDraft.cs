using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    public class RoutesDraft : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        [DisplayName("Route Name")]
        public string RouteName { get; set; }

        [Required]
        [DisplayName("Unit ID")]
        public Guid UnitId { get; set; }

        [ForeignKey("UnitId")]
        public virtual UnitDeployment UnitDeployment { get; set; }

        [Required]
        [DisplayName("Waypoints")]
        public string WaypointsJson { get; set; } // JSON array of waypoints: [{"grid": "E5", "lat": 33.7219, "lng": 73.0589, "via": "road"}]

        [DisplayName("Is Committed")]
        public bool IsCommitted { get; set; } = false;

        [DisplayName("Total Distance (km)")]
        public decimal TotalDistanceKm { get; set; } = 0;

        [DisplayName("Estimated Time (turns)")]
        public int EstimatedTimeTurns { get; set; } = 1;

        [DisplayName("Supply Impact")]
        public decimal SupplyImpact { get; set; } = 0;

        [MaxLength(50)]
        [DisplayName("Route Type")]
        public string RouteType { get; set; } = "Movement"; // Movement, Assembly, Withdrawal

        [MaxLength(20)]
        [DisplayName("Status")]
        public string Status { get; set; } = "Draft"; // Draft, Validated, Committed, Executed, Cancelled

        [MaxLength(500)]
        [DisplayName("Notes")]
        public string Notes { get; set; }

        [DisplayName("Created By User")]
        public string CreatedByUser { get; set; }

        [DisplayName("Committed Date")]
        public DateTime? CommittedDate { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }
    }
}
