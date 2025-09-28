using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    public class WarGameScenario : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string ScenarioCode { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } // Planning, Active, Paused, Completed

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public Guid? GameSessionId { get; set; }

        [ForeignKey("GameSessionId")]
        public virtual GameSession GameSession { get; set; }

        // Navigation properties
        public virtual ICollection<UnitDeployment> UnitDeployments { get; set; } = new List<UnitDeployment>();
        public virtual ICollection<Battle> Battles { get; set; } = new List<Battle>();
        public virtual ICollection<MovementOrder> MovementOrders { get; set; } = new List<MovementOrder>();
        public virtual ICollection<Objective> Objectives { get; set; } = new List<Objective>();
    }

    public class UnitDeployment : BaseEntity
    {
        [Required]
        public Guid? ScenarioId { get; set; }

        [ForeignKey("ScenarioId")]
        public virtual WarGameScenario Scenario { get; set; }

        [Required]
        public Guid? UnitId { get; set; }
        [Required]
        public Guid? TokenId { get; set; }

        [Required]
        [MaxLength(50)]
        public string UnitType { get; set; } // Infantry, Armoured, Artillery

        [Required]
        [MaxLength(100)]
        public string UnitName { get; set; }

        [Required]
        [MaxLength(50)]
        public string ForceType { get; set; } // Blue, Red

        [Required]
        public string Position { get; set; } // JSON: {lat: 0, lng: 0}

        [MaxLength(50)]
        public string Formation { get; set; } // Line, Column, Wedge, etc.

        [MaxLength(20)]
        public string Status { get; set; } // Deployed, Moving, InCombat, Destroyed, Withdrawn

        public int CurrentStrength { get; set; }
        public int MaxStrength { get; set; }

        public decimal Morale { get; set; } = 100; // 0-100
        public decimal Fatigue { get; set; } = 0; // 0-100

        // Enhanced Movement Properties
        public double MovementPoints { get; set; } = 30.0; // km/turn default
        public string CurrentTerrain { get; set; } = "Road"; // TerrainType
        public double RemainingMovement { get; set; } = 30.0; // Current turn movement

        // Enhanced Combat Properties
        public double CombatPower { get; set; } = 1.0; // Base combat effectiveness
        public double TerrainModifier { get; set; } = 1.0; // Current terrain bonus/penalty
        public double SupplyModifier { get; set; } = 1.0; // Supply state modifier

        // Enhanced Casualty Tracking
        public double StrengthPercentage { get; set; } = 100.0; // 0-100%
        public double EffectiveCombatPower { get; set; } = 1.0; // Dynamic combat power

        // Enhanced Supply State
        public int SupplyState { get; set; } = 100; // 100=Green, 75=Amber, 50=Red

        // Navigation properties
        public virtual ICollection<MovementOrder> MovementOrders { get; set; } = new List<MovementOrder>();
        public virtual ICollection<BattleParticipant> BattleParticipations { get; set; } = new List<BattleParticipant>();

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

        public void UpdateFromMilitaryUnit(MilitaryUnit unit)
        {
            StrengthPercentage = unit.StrengthPercentage;
            EffectiveCombatPower = unit.EffectiveCombatPower;
            CurrentStrength = (int)(MaxStrength * (StrengthPercentage / 100.0));
            CombatPower = unit.CombatPower;
            TerrainModifier = unit.TerrainModifier;
            SupplyModifier = unit.SupplyModifier;
            SupplyState = unit.SupplyState;
            MovementPoints = unit.MovementPoints;
            CurrentTerrain = unit.CurrentTerrain;
            RemainingMovement = unit.RemainingMovement;
        }
    }

    public class MovementOrder : BaseEntity
    {
        [Required]
        public Guid? UnitDeploymentId { get; set; }

        [ForeignKey("UnitDeploymentId")]
        public virtual UnitDeployment UnitDeployment { get; set; }

        [Required]
        public string StartPosition { get; set; } // JSON: {lat: 0, lng: 0}

        [Required]
        public string EndPosition { get; set; } // JSON: {lat: 0, lng: 0}

        public string Waypoints { get; set; } // JSON array of positions
        public string Notes { get; set; } 

        [Required]
        [MaxLength(20)]
        public string MovementType { get; set; } // March, Tactical, Combat

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } // Planned, InProgress, Completed, Cancelled

        public string EngagementRule { get; set; } 

        public DateTime? StartTime { get; set; }
        public DateTime? EstimatedArrival { get; set; }
        public DateTime? ActualArrival { get; set; }

        public decimal Speed { get; set; } // km/h
        public decimal Distance { get; set; } // km

        [MaxLength(200)]
        public string TerrainFactors { get; set; } // JSON of terrain modifiers

        [MaxLength(50)]
        public string IssuedByUserId { get; set; }

        [MaxLength(50)]
        public string IssuedByUserName { get; set; }
    }

    public class Battle : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string BattleName { get; set; }

        [Required]
        public Guid? ScenarioId { get; set; }

        [ForeignKey("ScenarioId")]
        public virtual WarGameScenario Scenario { get; set; }

        [Required]
        public string BattleLocation { get; set; } // JSON: {lat: 0, lng: 0, radius: 0}

        [Required]
        [MaxLength(20)]
        public string BattleType { get; set; } // Engagement, Assault, Defense, Withdrawal

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } // Planned, Active, Resolved

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        [MaxLength(50)]
        public string Victor { get; set; } // Blue, Red, Draw

        public string TerrainType { get; set; }
        public decimal TerrainModifier { get; set; } = 1.0m;

        public string WeatherConditions { get; set; }
        public decimal WeatherModifier { get; set; } = 1.0m;

        public string BattleResults { get; set; } // JSON of detailed results

        [MaxLength(1000)]
        public string BattleLog { get; set; }

        // Navigation properties
        public virtual ICollection<BattleParticipant> Participants { get; set; } = new List<BattleParticipant>();
        public virtual ICollection<CombatResult> CombatResults { get; set; } = new List<CombatResult>();
    }

    public class BattleParticipant : BaseEntity
    {
        [Required]
        public Guid BattleId { get; set; }

        [ForeignKey("BattleId")]
        public virtual Battle Battle { get; set; }

        [Required]
        public Guid UnitDeploymentId { get; set; }

        [ForeignKey("UnitDeploymentId")]
        public virtual UnitDeployment UnitDeployment { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } // Attacker, Defender, Support

        public int InitialStrength { get; set; }
        public int FinalStrength { get; set; }
        public int Casualties { get; set; }

        public decimal CombatEffectiveness { get; set; } = 1.0m;
        public decimal ProtectionFactor { get; set; } = 1.0m;

        [MaxLength(50)]
        public string ProtectionType { get; set; } // From ForceProtection table

        public string Equipment { get; set; } // JSON of weapons and equipment used

        public string Position { get; set; } // JSON: {lat: 0, lng: 0}
    }

    public class CombatResult : BaseEntity
    {
        [Required]
        public Guid BattleId { get; set; }

        [ForeignKey("BattleId")]
        public virtual Battle Battle { get; set; }

        [Required]
        public Guid? AttackerId { get; set; }

        [Required]
        public Guid? DefenderId { get; set; }

        [MaxLength(20)]
        public string CombatType { get; set; } // Direct, Artillery, ATGM, etc.

        public decimal AttackerStrength { get; set; }
        public decimal DefenderStrength { get; set; }

        public decimal AttackerLosses { get; set; }
        public decimal DefenderLosses { get; set; }

        public decimal TerrainModifier { get; set; } = 1.0m;
        public decimal ProtectionModifier { get; set; } = 1.0m;

        [MaxLength(20)]
        public string Result { get; set; } // AttackerWins, DefenderWins, Draw

        public DateTime CombatTime { get; set; }

        public string CombatDetails { get; set; } // JSON of detailed combat calculations
    }

    public class Objective : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string ObjectiveName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public Guid ScenarioId { get; set; }

        [ForeignKey("ScenarioId")]
        public virtual WarGameScenario Scenario { get; set; }

        [Required]
        public string ObjectiveLocation { get; set; } // JSON: {lat: 0, lng: 0, radius: 0}

        [Required]
        [MaxLength(20)]
        public string ObjectiveType { get; set; } // Capture, Defend, Destroy, Reach

        [Required]
        [MaxLength(50)]
        public string AssignedToForce { get; set; } // Blue, Red

        [MaxLength(20)]
        public string Status { get; set; } // Pending, InProgress, Completed, Failed

        public int PointValue { get; set; } = 0;

        public DateTime? CompletedAt { get; set; }

        [MaxLength(50)]
        public string CompletedByTeam { get; set; }
    }

    public class SimulationEvent : BaseEntity
    {
        [Required]
        public Guid ScenarioId { get; set; }

        [ForeignKey("ScenarioId")]
        public virtual WarGameScenario Scenario { get; set; }

        [Required]
        [MaxLength(50)]
        public string EventType { get; set; } // Movement, Combat, Objective, Communication

        [Required]
        public string EventData { get; set; } // JSON of event details

        public DateTime EventTime { get; set; }

        [MaxLength(50)]
        public string TriggeredByUserId { get; set; }

        [MaxLength(50)]
        public string TriggeredByUserName { get; set; }

        [MaxLength(50)]
        public string AffectedTeamId { get; set; }

        public bool IsProcessed { get; set; } = false;
    }

    public class CombatCalculation
    {
        public decimal AttackerFirepower { get; set; }
        public decimal DefenderFirepower { get; set; }
        public decimal AttackerProtection { get; set; }
        public decimal DefenderProtection { get; set; }
        public decimal TerrainModifier { get; set; }
        public decimal WeatherModifier { get; set; }
        public decimal MoraleModifier { get; set; }
        public decimal FatigueModifier { get; set; }
        public decimal AttackerCasualties { get; set; }
        public decimal DefenderCasualties { get; set; }
        public string Victor { get; set; }
        public decimal VictoryMargin { get; set; }
    }

    public class MovementCalculation
    {
        public decimal BaseSpeed { get; set; }
        public decimal TerrainModifier { get; set; }
        public decimal WeatherModifier { get; set; }
        public decimal FatigueModifier { get; set; }
        public decimal EffectiveSpeed { get; set; }
        public decimal Distance { get; set; }
        public decimal TravelTime { get; set; } // in hours
        public string TerrainTypes { get; set; }
    }
}
