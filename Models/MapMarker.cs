using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    [Table("MapMarkers")]
    public class MapMarker : BaseEntity
    {
        [Required]
        [DisplayName("Token ID")]
        [ForeignKey("Token")]
        public Guid? TokenId { get; set; }

        // Phase 01 Specification - GUID field
        [Required]
        [DisplayName("Token ID GUID")]
        public Guid TokenId_GUID { get; set; }  

        [Required]
        [DisplayName("Position")]
        public string Position { get; set; } // JSON: {lat: 0, lng: 0} or grid coordinates

        [Required]
        public string latitude { get; set; } 

        [Required]
        public string longitude { get; set; } 

        [DisplayName("Is Selected")]
        public bool IsSelected { get; set; } = false;

        [DisplayName("Z Index")]
        public int ZIndex { get; set; } = 0;

        [MaxLength(100)]
        [DisplayName("Marker Type")]
        public string MarkerType { get; set; } = "Unit"; // Unit, Objective, Waypoint, etc.

        [MaxLength(500)]
        [DisplayName("Description")]
        public string Description { get; set; }

        public virtual Token Token { get; set; } = null!;
    }
}
