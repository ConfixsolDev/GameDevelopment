using System.ComponentModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    public class MilitaryUnit : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        [DisplayName("Unit Name")]
        public string Name { get; set; }

        [MaxLength(500)]
        [DisplayName("Description")]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        [DisplayName("Unit Code")]
        public string UnitCode { get; set; }

        [Required]
        [DisplayName("Strength")]
        public int Strength { get; set; }

        [Required]
        [MaxLength(50)]
        [DisplayName("Force Type")]
        public string ForceType { get; set; } // Blue or Red

        [MaxLength(50)]
        [DisplayName("Unit Type")]
        public string UnitType { get; set; } // Infantry, Armoured, Artillery

        // Link to Brigade
        [DisplayName("Brigade ID")]
        public Guid? BrigadeId { get; set; }

        [DisplayName("Token ID")]
        public Guid? TokenId { get; set; }

        [ForeignKey("BrigadeId")]
        public virtual Brigade Brigade { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }

        // Enhanced Movement Properties
        [DisplayName("Movement Points (km/turn)")]
        public double MovementPoints { get; set; } = 30.0; // km/turn default

        [MaxLength(50)]
        [DisplayName("Current Terrain")]
        public string CurrentTerrain { get; set; } = "Road"; // TerrainType

        [DisplayName("Remaining Movement")]
        public double RemainingMovement { get; set; } = 30.0; // Current turn movement

        // Enhanced Combat Properties
        [DisplayName("Combat Power")]
        public double CombatPower { get; set; } = 1.0; // Base combat effectiveness

        [DisplayName("Terrain Modifier")]
        public double TerrainModifier { get; set; } = 1.0; // Current terrain bonus/penalty

        [DisplayName("Supply Modifier")]
        public double SupplyModifier { get; set; } = 1.0; // Supply state modifier

        // Enhanced Casualty Tracking
        [DisplayName("Strength Percentage")]
        public double StrengthPercentage { get; set; } = 100.0; // 0-100%

        [DisplayName("Effective Combat Power")]
        public double EffectiveCombatPower { get; set; } = 1.0; // Dynamic combat power

        // Enhanced Supply State
        [DisplayName("Supply State")]
        public int SupplyState { get; set; } = 100; // 100=Green, 75=Amber, 50=Red

        // Helper Methods
        public double GetEffectiveCombatPower()
        {
            if (StrengthPercentage >= 50)
                return CombatPower;
            else
                return CombatPower * (StrengthPercentage / 100.0) * 0.5; // Degraded effectiveness
        }

        public void UpdateSupplyModifier()
        {
            SupplyModifier = SupplyState / 100.0;
        }
    }

    public class InfantryBattalion : MilitaryUnit
    {
        [DisplayName("Companies")]
        public int Companies { get; set; }
        
        [DisplayName("Anti-Tank Guided Missiles")]
        public int ATGMS { get; set; }
        
        [DisplayName("Rocket Launchers")]
        public int RocketLauncher { get; set; }
        
        [DisplayName("81mm Mortars")]
        public int Mortars81mm { get; set; }
        
        [DisplayName("120mm Mortars")]
        public int Mortars120mm { get; set; }
        
        [DisplayName("Grenade Launchers")]
        public int GrenadeLaunchers { get; set; }
        
        [DisplayName("Heavy Machine Guns / Automatic Grenade Launchers")]
        public int HMG_AGL { get; set; }
        
        [DisplayName("Machine Guns / Light Machine Guns")]
        public int MG_LMG { get; set; }
        
        [DisplayName("Man-Portable Air Defense Systems")]
        public int MANPADS { get; set; }
        
        [DisplayName("Grenades")]
        public int Grenades { get; set; }

        // Drone Data
        [DisplayName("Drones")]
        public int Drones { get; set; }
        
        [MaxLength(200)]
        [DisplayName("Drone Types")]
        public string DroneTypes { get; set; }

        // Mobility Data
        [DisplayName("Marching Speed - Trucks on Roads (km/h)")]
        public decimal MarchingSpeedTrucksRoads { get; set; } = 30; // kmph
        
        [DisplayName("Marching Speed - APCs (km/h)")]
        public decimal MarchingSpeedAPCs { get; set; } = 20; // kmph
        
        [DisplayName("Marching Speed - Cross Country (km/h)")]
        public decimal MarchingSpeedCrossCountry { get; set; } = 5; // kmph
        
        [DisplayName("Marching Speed - APCs Cross Country (km/h)")]
        public decimal MarchingSpeedAPCsCrossCountry { get; set; } = 10; // kmph
        
        [DisplayName("Combat Advance Speed (km/h)")]
        public decimal CombatAdvanceSpeed { get; set; } = 1; // kmph average

        public InfantryBattalion()
        {
            UnitType = "Infantry";
        }
    }

    public class ArmouredRegiment : MilitaryUnit
    {
        [DisplayName("Squadrons")]
        public int Squadrons { get; set; }
        
        [DisplayName("Tanks")]
        public int Tanks { get; set; }
        
        [DisplayName("Anti-Tank Guided Missiles")]
        public int ATGMS { get; set; }
        
        [DisplayName("120mm Mortars")]
        public int Mortars120mm { get; set; }
        
        [DisplayName("Heavy Machine Guns")]
        public int HMG { get; set; }

        // Drone Data
        [DisplayName("Drones")]
        public int Drones { get; set; }
        
        [MaxLength(200)]
        [DisplayName("Drone Types")]
        public string DroneTypes { get; set; }

        // Armour Speed Data
        [DisplayName("Marching Speed - Roads (km/h)")]
        public decimal MarchingSpeedRoads { get; set; } = 15; // kmph
        
        [DisplayName("Marching Speed - Cross Country (km/h)")]
        public decimal MarchingSpeedCrossCountry { get; set; } = 10; // kmph
        
        [DisplayName("Combat Advance Speed (km/h)")]
        public decimal CombatAdvanceSpeed { get; set; } = 1; // kmph average

        public bool IsDeleted { get; set; }
        public ArmouredRegiment()
        {
            UnitType = "Armoured";
        }
    }

    public class ArtilleryRegiment : MilitaryUnit
    {
        [DisplayName("Batteries")]
        public int Batteries { get; set; }
        
        [DisplayName("Guns")]
        public int Guns { get; set; }
        
        [DisplayName("Gun Range (km)")]
        public decimal GunRange { get; set; } // km
        
        [DisplayName("Heavy Machine Guns")]
        public int HMG { get; set; }
        
        [DisplayName("Gun Caliber")]
        public string GunCaliber { get; set; } = "155mm SP";

        // Drone Data
        [DisplayName("Drones")]
        public int Drones { get; set; }
        
        [MaxLength(200)]
        [DisplayName("Drone Types")]
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
        [DisplayName("Terrain Type")]
        public string TerrainType { get; set; }

        [Required]
        [DisplayName("X Factor")]
        public decimal XFactor { get; set; }

        [MaxLength(200)]
        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Tank PSI Minimum")]
        public decimal TankPSIMin { get; set; }
        
        [DisplayName("Tank PSI Maximum")]
        public decimal TankPSIMax { get; set; }
        
        [DisplayName("APC PSI Minimum")]
        public decimal APCPsimMin { get; set; }
        
        [DisplayName("APC PSI Maximum")]
        public decimal APCPsimMax { get; set; }


        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }
    }

    public class ForceProtection : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        [DisplayName("Force Type")]
        public string ForceType { get; set; } // Blue or Red

        [Required]
        [MaxLength(100)]
        [DisplayName("Protection Type")]
        public string ProtectionType { get; set; } // Open in Field, Open Trenches, etc.

        [Required]
        [MaxLength(50)]
        [DisplayName("Degree of Preparation")]
        public string DegreeOfPreparation { get; set; } // Hasty, Prep with OHP, RCC OHP, etc.

        [DisplayName("Protection Factor")]
        public decimal? ProtectionFactor { get; set; }

        [MaxLength(200)]
        [DisplayName("Notes")]
        public string Notes { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }
    }

    public class Brigade : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        [DisplayName("Brigade Name")]
        public string Name { get; set; }

        [MaxLength(500)]
        [DisplayName("Description")]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        [DisplayName("Brigade Code")]
        public string BrigadeCode { get; set; }

        [Required]
        [MaxLength(50)]
        [DisplayName("Force Type")]
        public string ForceType { get; set; } // Blue or Red

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }

        // Optional: Link to map token
        [DisplayName("Token ID")]
        public Guid? TokenId { get; set; }

        [ForeignKey("TokenId")]
        public virtual Token Token { get; set; }

        // Navigation properties for units under this brigade
        public virtual ICollection<InfantryBattalion> InfantryBattalions { get; set; } = new List<InfantryBattalion>();
        public virtual ICollection<ArmouredRegiment> ArmouredRegiments { get; set; } = new List<ArmouredRegiment>();
        public virtual ICollection<ArtilleryRegiment> ArtilleryRegiments { get; set; } = new List<ArtilleryRegiment>();
    }
}
