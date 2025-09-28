using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    [Table("MapMarkers")]
    public class MapMarker:BaseEntity
    {
        [Required]
        [ForeignKey("Token")]
        public Guid? TokenId { get; set; }  

        [Required]
        public string latitude { get; set; } 

        [Required]
        public string longitude { get; set; } 
        public virtual Token Token { get; set; } = null!;
    }
}
