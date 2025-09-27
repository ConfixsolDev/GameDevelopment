using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    public abstract class MilitaryUnit : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string UnitCode { get; set; }

        [Required]
        public int Strength { get; set; }

        [Required]
        [MaxLength(50)]
        public string ForceType { get; set; } // Blue or Red

        [MaxLength(50)]
        public string UnitType { get; set; } // Infantry, Armoured, Artillery

        // Link to Brigade
        public Guid? BrigadeId { get; set; }

        public Guid? TokenId { get; set; }

        [ForeignKey("BrigadeId")]
        public virtual Brigade Brigade { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }
    }

    public class InfantryBattalion : MilitaryUnit
    {
        public int Companies { get; set; }
        public int ATGMS { get; set; }
        public int RocketLauncher { get; set; }
        public int Mortars81mm { get; set; }
        public int Mortars120mm { get; set; }
        public int GrenadeLaunchers { get; set; }
        public int HMG_AGL { get; set; }
        public int MG_LMG { get; set; }
        public int MANPADS { get; set; }
        public int Grenades { get; set; }

        // Drone Data
        public int Drones { get; set; }
        [MaxLength(200)]
        public string DroneTypes { get; set; }

        // Mobility Data
        public decimal MarchingSpeedTrucksRoads { get; set; } = 30; // kmph
        public decimal MarchingSpeedAPCs { get; set; } = 20; // kmph
        public decimal MarchingSpeedCrossCountry { get; set; } = 5; // kmph
        public decimal MarchingSpeedAPCsCrossCountry { get; set; } = 10; // kmph
        public decimal CombatAdvanceSpeed { get; set; } = 1; // kmph average

        public InfantryBattalion()
        {
            UnitType = "Infantry";
        }
    }

    public class ArmouredRegiment : MilitaryUnit
    {
        public int Squadrons { get; set; }
        public int Tanks { get; set; }
        public int ATGMS { get; set; }
        public int Mortars120mm { get; set; }
        public int HMG { get; set; }

        // Drone Data
        public int Drones { get; set; }
        [MaxLength(200)]
        public string DroneTypes { get; set; }

        // Armour Speed Data
        public decimal MarchingSpeedRoads { get; set; } = 15; // kmph
        public decimal MarchingSpeedCrossCountry { get; set; } = 10; // kmph
        public decimal CombatAdvanceSpeed { get; set; } = 1; // kmph average

        public bool IsDeleted { get; set; }
        public ArmouredRegiment()
        {
            UnitType = "Armoured";
        }
    }

    public class ArtilleryRegiment : MilitaryUnit
    {
        public int Batteries { get; set; }
        public int Guns { get; set; }
        public decimal GunRange { get; set; } // km
        public int HMG { get; set; }
        public string GunCaliber { get; set; } = "155mm SP";

        // Drone Data
        public int Drones { get; set; }
        [MaxLength(200)]
        public string DroneTypes { get; set; }

        public ArtilleryRegiment()
        {
            UnitType = "Artillery";
        }
    }

    public class TerrainMobilityFactor : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string TerrainType { get; set; }

        [Required]
        public decimal XFactor { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        public decimal TankPSIMin { get; set; }
        public decimal TankPSIMax { get; set; }
        public decimal APCPsimMin { get; set; }
        public decimal APCPsimMax { get; set; }


        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }
    }

    public class ForceProtection : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string ForceType { get; set; } // Blue or Red

        [Required]
        [MaxLength(100)]
        public string ProtectionType { get; set; } // Open in Field, Open Trenches, etc.

        [Required]
        [MaxLength(50)]
        public string DegreeOfPreparation { get; set; } // Hasty, Prep with OHP, RCC OHP, etc.

        public decimal? ProtectionFactor { get; set; }

        [MaxLength(200)]
        public string Notes { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }
    }

    public class Brigade : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string BrigadeCode { get; set; }

        [Required]
        [MaxLength(50)]
        public string ForceType { get; set; } // Blue or Red

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }

        // Optional: Link to map token
        public Guid? TokenId { get; set; }

        [ForeignKey("TokenId")]
        public virtual Token Token { get; set; }

        // Navigation properties for units under this brigade
        public virtual ICollection<InfantryBattalion> InfantryBattalions { get; set; } = new List<InfantryBattalion>();
        public virtual ICollection<ArmouredRegiment> ArmouredRegiments { get; set; } = new List<ArmouredRegiment>();
        public virtual ICollection<ArtilleryRegiment> ArtilleryRegiments { get; set; } = new List<ArtilleryRegiment>();
    }
}
