using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace WargameBoard.Core.Entities
{
    // ---------------- Enums ----------------
    public enum WeatherType { Clear = 1, Overcast = 2, Rain = 3, Storm = 4, Fog = 5 }
    public enum Posture { Defensive = 1, Offensive = 2, Reconnaissance = 3, Support = 4, Reserve = 5 }
    public enum VictoryConditionKind { SeizeByTurn = 1, HoldToEnd = 2, DestroyUnits = 3, ExitUnits = 4 , Control = 5 , Destroy = 6 , Defend = 7 , Capture = 8 , Reach = 9, Survive = 10 }
    public enum FeatureKind { Fort = 1, Obstacle = 2 }
    public enum QualityLevel { Green = 1, Regular = 2, Veteran = 3, Elite = 4 }

    public enum GainLoss { Gained = 1, Lost = 2 }

    // ---------------- Admin / Lookups ----------------
    public class Side
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(7)]
        public string? Color { get; set; } // #RRGGBB

        public bool IsActive { get; set; } = true;

        // Optional: sides referenced by features/objectives/units etc.
        public ICollection<HexFeature> HexFeatures { get; set; } = new List<HexFeature>();
    }

    public class UnitType
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;
        [StringLength(200)]
        public string? Description { get; set; }
        [StringLength(20)]
        public string? Category { get; set; } // Infantry / Armor / Support
        public bool IsActive { get; set; } = true;
    }

    public class MovementProfile
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;
        [StringLength(200)]
        public string? Description { get; set; }

        [Range(1, 120)] public int RoadKmph { get; set; } = 15;
        [Range(1, 120)] public int XCountryKmph { get; set; } = 5;
        [Range(1, 10)] public int CombatAdvanceKmph { get; set; } = 1;

        public bool IsActive { get; set; } = true;
    }

    public class TerrainType
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;
        [StringLength(200)]
        public string? Description { get; set; }
        [StringLength(200)]
        public string? Code { get; set; }
        [StringLength(7)]
        public string? Color { get; set; }
        [Range(0, 10)]
        public int MovementCost { get; set; } = 1;
        [Range(0, 10)]
        public int DefenseModifier { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    public class FortificationType
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;
        [StringLength(500)]
        public string? Description { get; set; }
        public ICollection<HexFeature> HexFeatures { get; set; } = new List<HexFeature>();
    }

    public class ObstacleType
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;
        [StringLength(500)]
        public string? Description { get; set; }
        public ICollection<HexFeature> HexFeatures { get; set; } = new List<HexFeature>();
    }

    public class TokenGroup
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;
        [StringLength(200)]
        public string? Description { get; set; }
        [StringLength(20)]
        public string? Category { get; set; } // Unit / Objective / Status
        [Range(1, 100)]
        public decimal DefaultWidthMm { get; set; } = 25;
        [Range(1, 100)]
        public decimal DefaultHeightMm { get; set; } = 25;
        public bool IsActive { get; set; } = true;
    }

    // ---------------- Map ----------------
    public class Hex
    {
        public int Id { get; set; }
        public int Q { get; set; }
        public int R { get; set; }

        public int? TerrainTypeId { get; set; }
        public TerrainType? TerrainType { get; set; } = null!;

        [StringLength(100)]
        public string? KeyFeature { get; set; } // Bridge, Town, Crossroads

        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
        public ICollection<HexFeature> HexFeatures { get; set; } = new List<HexFeature>();
        public ICollection<ScenarioUnit> ScenarioUnits { get; set; } = new List<ScenarioUnit>();
        public ICollection<ScenarioObjective> ScenarioObjectives { get; set; } = new List<ScenarioObjective>();
        public ICollection<BoardCell> BoardCells { get; set; } = new List<BoardCell>();
        public ICollection<Placement> Placements { get; set; } = new List<Placement>();
        public ICollection<MoveEvent> MoveEventsFrom { get; set; } = new List<MoveEvent>();
        public ICollection<MoveEvent> MoveEventsTo { get; set; } = new List<MoveEvent>();

        [NotMapped]
        public string CoordLabel => $"{Q},{R}";
    }

    public class HexFeature
    {
        public int Id { get; set; }

        public int HexId { get; set; }
        public Hex Hex { get; set; } = null!;

        public FeatureKind FeatureKind { get; set; } = FeatureKind.Fort;

        public int? FortificationTypeId { get; set; }
        public FortificationType? FortificationType { get; set; }

        public int? ObstacleTypeId { get; set; }
        public ObstacleType? ObstacleType { get; set; }

        public int? SideId { get; set; }
        public Side? Side { get; set; }
    }

    // ---------------- Forces ----------------
    public class Unit
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public int SideId { get; set; }
        public Side? Side { get; set; }

        public int UnitTypeId { get; set; }
        public UnitType? UnitType { get; set; }

        public int? Personnel { get; set; }
        public int? VehiclesPrimary { get; set; }

        public QualityLevel Quality { get; set; } = QualityLevel.Regular;
        [Range(0, 100)]
        public int Cohesion { get; set; } = 70;

        public int? MovementProfileId { get; set; }
        public MovementProfile? MovementProfile { get; set; }

        public UnitCapability? Capability { get; set; } // 1:1
    }

    public class UnitCapability
    {
        [Key] public int UnitId { get; set; }
        public Unit Unit { get; set; } = null!;

        [Range(0, 100)] public int AttackSoft { get; set; }
        [Range(0, 100)] public int AttackHard { get; set; }
        [Range(0, 100)] public int Defense { get; set; }
        [Range(0, 100)] public int IndirectSupport { get; set; }

        public int? AtgmCount { get; set; }
        public int? MortarsCount { get; set; }
        public int? RocketsCount { get; set; }
        public int? HmgCount { get; set; }
    }

    // ---------------- Scenario ----------------
    public class Scenario
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 480)]
        public int TurnLengthMinutes { get; set; } = 60;

        [Range(1, 100)]
        public int MaxTurns { get; set; } = 12;

        public WeatherType Weather { get; set; } = WeatherType.Clear;

        [StringLength(1000)]
        public string? Notes { get; set; }

        public ICollection<ScenarioUnit> ScenarioUnits { get; set; }
        public ICollection<ScenarioObjective> ScenarioObjectives { get; set; }
    }

    public class ScenarioUnit
    {
        public int Id { get; set; }

        public int ScenarioId { get; set; }
        public Scenario? Scenario { get; set; }

        public int UnitId { get; set; }
        public Unit? Unit { get; set; }

        public int StartHexId { get; set; }
        public Hex? StartHex { get; set; }

        [Range(0, 20)]
        public int Steps { get; set; }

        public Posture Posture { get; set; } = Posture.Defensive;

        public bool Hidden { get; set; }
    }

    public class ScenarioObjective
    {
        public int Id { get; set; }

        public int ScenarioId { get; set; }
        public Scenario? Scenario { get; set; }

        public int HexId { get; set; }
        public Hex? Hex { get; set; }

        public int SideId { get; set; }
        public Side? Side { get; set; }

        [Range(1, 100)]
        public int VictoryPoints { get; set; }

        public VictoryConditionKind ConditionKind { get; set; } = VictoryConditionKind.HoldToEnd;

        public int? TurnThreshold { get; set; } // only for SeizeByTurn

        public ICollection<ObjectiveControlLog> Logs { get; set; }
    }

    // ---------------- Tokens ----------------
    public class TokenDesign
    {
        public int Id { get; set; }
        public int TokenGroupId { get; set; }
        public TokenGroup? TokenGroup { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public int? DefaultSideId { get; set; }
        public Side? DefaultSide { get; set; }

        [Range(1, 100)]
        public decimal WidthMm { get; set; } = 25;
        [Range(1, 100)]
        public decimal HeightMm { get; set; } = 25;

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class TokenPiece
    {
        public int Id { get; set; }
        public int TokenDesignId { get; set; }
        public TokenDesign? TokenDesign { get; set; }

        public int? SideId { get; set; }
        public Side? Side { get; set; }

        [StringLength(50)]
        public string? Serial { get; set; }

        [StringLength(100)]
        public string? HardwareIdentity { get; set; } // unique (can be null)

        public bool IsUnique { get; set; }
    }

    // ---------------- Runtime ----------------
    public class Session
    {
        public int Id { get; set; }

        public int ScenarioId { get; set; }
        public Scenario? Scenario { get; set; }

        public int? CurrentSideId { get; set; }
        public Side? CurrentSide { get; set; }

        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public ICollection<Turn> Turns { get; set; }
        public ICollection<Placement> Placements { get; set; }
        public ICollection<SessionAssignment> Assignments { get; set; }
        public ICollection<TouchEvent> TouchEvents { get; set; }
        public ICollection<ObjectiveControlLog> ObjectiveLogs { get; set; }
        public ICollection<MoveEvent> MoveEvents { get; set; } // needed by DbContext mapping
    }

    public class Turn
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;
        public int Number { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public ICollection<Placement> Placements { get; set; }
        public ICollection<MoveEvent> MoveEvents { get; set; }
        public ICollection<ObjectiveControlLog> ObjectiveLogs { get; set; }
    }

    public class Placement
    {
        public int Id { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        public int? TurnId { get; set; }          // NoAction FK → Turn
        public Turn? Turn { get; set; }

        public int TokenPieceId { get; set; }
        public TokenPiece? TokenPiece { get; set; }

        public int HexId { get; set; }
        public Hex? Hex { get; set; }

        public DateTime PlacedAt { get; set; }
        public int? PlacedByUserId { get; set; }
    }

    public class MoveEvent
    {
        public int Id { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        public int? TurnId { get; set; }          // NoAction FK → Turn
        public Turn? Turn { get; set; }

        public int TokenPieceId { get; set; }
        public TokenPiece TokenPiece { get; set; } = null!;

        public int FromHexId { get; set; }
        public Hex FromHex { get; set; } = null!;

        public int ToHexId { get; set; }
        public Hex ToHex { get; set; } = null!;

        public DateTime Timestamp { get; set; }
    }

    public class ObjectiveControlLog
    {
        public int Id { get; set; }

        public int SessionId { get; set; }      // NoAction FK in model config
        public Session Session { get; set; } = null!;

        public int? TurnId { get; set; }        // NoAction FK in model config
        public Turn? Turn { get; set; }

        public int ObjectiveId { get; set; }
        public ScenarioObjective Objective { get; set; } = null!;

        public int SideId { get; set; }
        public Side Side { get; set; } = null!;

        public GainLoss GainedLost { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class SessionAssignment
    {
        public int Id { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        public int TokenPieceId { get; set; }
        public TokenPiece TokenPiece { get; set; } = null!;

        // Flexible targets (nullable)
        public int? ScenarioUnitId { get; set; }
        public ScenarioUnit? ScenarioUnit { get; set; }

        public int? HexFeatureId { get; set; }
        public HexFeature? HexFeature { get; set; }

        public int? ScenarioObjectiveId { get; set; }
        public ScenarioObjective? ScenarioObjective { get; set; }

        public DateTime AssignedAt { get; set; }
        public int? AssignedByUserId { get; set; }
    }

    // ---------------- Board / Touch ----------------
    public class Board
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [StringLength(300)]
        public string? Description { get; set; }

        public ICollection<BoardCell> Cells { get; set; } = new List<BoardCell>();
    }

    public class BoardCell
    {
        public int Id { get; set; }
        public int BoardId { get; set; }
        public Board Board { get; set; } = null!;

        public int Row { get; set; }
        public int Col { get; set; }

        [StringLength(50)]
        public string? SensorAddress { get; set; }

        public int? HexId { get; set; }
        public Hex? Hex { get; set; }

        // Capacitive settings (optional)
        public double? Threshold { get; set; }

        // Telemetry
        public int? LastStrength { get; set; }
        public DateTime? LastStrengthUpdate { get; set; }
    }

    public class TouchEvent
    {
        public int Id { get; set; }

        public int BoardCellId { get; set; }
        public BoardCell BoardCell { get; set; } = null!;

        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        // Raw sensor values
        public int Strength { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
