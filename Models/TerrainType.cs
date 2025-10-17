using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    public class TerrainType : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        [DisplayName("Terrain Code")]
        public string TerrainCode { get; set; }

        [Required]
        [MaxLength(100)]
        [DisplayName("Terrain Name")]
        public string Name { get; set; }

        [DisplayName("Movement Cost - Road")]
        public decimal MovementCostRoad { get; set; } = 1.0m;

        [DisplayName("Movement Cost - Cross Country")]
        public decimal MovementCostCrossCountry { get; set; } = 1.2m;

        [DisplayName("Combat Modifier")]
        public decimal CombatModifier { get; set; } = 1.0m;

        [MaxLength(500)]
        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Is Passable")]
        public bool IsPassable { get; set; } = true;

        [DisplayName("Is Impassable to Vehicles")]
        public bool IsImpassableToVehicles { get; set; } = false;

        [DisplayName("Visibility Modifier")]
        public decimal VisibilityModifier { get; set; } = 1.0m;

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }
    }
}
