using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WargameBoard.Web.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Boards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FortificationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FortificationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovementProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RoadKmph = table.Column<int>(type: "int", nullable: false),
                    XCountryKmph = table.Column<int>(type: "int", nullable: false),
                    CombatAdvanceKmph = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ObstacleTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObstacleTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scenarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TurnLengthMinutes = table.Column<int>(type: "int", nullable: false),
                    MaxTurns = table.Column<int>(type: "int", nullable: false),
                    Weather = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scenarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sides", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TerrainTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    MovementCost = table.Column<int>(type: "int", nullable: false),
                    DefenseModifier = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerrainTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TokenGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DefaultWidthMm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DefaultHeightMm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioId = table.Column<int>(type: "int", nullable: false),
                    CurrentSideId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sessions_Sides_CurrentSideId",
                        column: x => x.CurrentSideId,
                        principalTable: "Sides",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Hexes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Q = table.Column<int>(type: "int", nullable: false),
                    R = table.Column<int>(type: "int", nullable: false),
                    TerrainTypeId = table.Column<int>(type: "int", nullable: true),
                    KeyFeature = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hexes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hexes_TerrainTypes_TerrainTypeId",
                        column: x => x.TerrainTypeId,
                        principalTable: "TerrainTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TokenDesigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TokenGroupId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefaultSideId = table.Column<int>(type: "int", nullable: true),
                    WidthMm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HeightMm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenDesigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenDesigns_Sides_DefaultSideId",
                        column: x => x.DefaultSideId,
                        principalTable: "Sides",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TokenDesigns_TokenGroups_TokenGroupId",
                        column: x => x.TokenGroupId,
                        principalTable: "TokenGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SideId = table.Column<int>(type: "int", nullable: false),
                    UnitTypeId = table.Column<int>(type: "int", nullable: false),
                    Personnel = table.Column<int>(type: "int", nullable: true),
                    VehiclesPrimary = table.Column<int>(type: "int", nullable: true),
                    Quality = table.Column<int>(type: "int", nullable: false),
                    Cohesion = table.Column<int>(type: "int", nullable: false),
                    MovementProfileId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Units_MovementProfiles_MovementProfileId",
                        column: x => x.MovementProfileId,
                        principalTable: "MovementProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Units_Sides_SideId",
                        column: x => x.SideId,
                        principalTable: "Sides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Units_UnitTypes_UnitTypeId",
                        column: x => x.UnitTypeId,
                        principalTable: "UnitTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Turns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Turns_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoardCells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardId = table.Column<int>(type: "int", nullable: false),
                    Row = table.Column<int>(type: "int", nullable: false),
                    Col = table.Column<int>(type: "int", nullable: false),
                    SensorAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HexId = table.Column<int>(type: "int", nullable: true),
                    Threshold = table.Column<double>(type: "float", nullable: true),
                    LastStrength = table.Column<int>(type: "int", nullable: true),
                    LastStrengthUpdate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardCells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardCells_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BoardCells_Hexes_HexId",
                        column: x => x.HexId,
                        principalTable: "Hexes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "HexFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HexId = table.Column<int>(type: "int", nullable: false),
                    FeatureKind = table.Column<int>(type: "int", nullable: false),
                    FortificationTypeId = table.Column<int>(type: "int", nullable: true),
                    ObstacleTypeId = table.Column<int>(type: "int", nullable: true),
                    SideId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HexFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HexFeatures_FortificationTypes_FortificationTypeId",
                        column: x => x.FortificationTypeId,
                        principalTable: "FortificationTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HexFeatures_Hexes_HexId",
                        column: x => x.HexId,
                        principalTable: "Hexes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HexFeatures_ObstacleTypes_ObstacleTypeId",
                        column: x => x.ObstacleTypeId,
                        principalTable: "ObstacleTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HexFeatures_Sides_SideId",
                        column: x => x.SideId,
                        principalTable: "Sides",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScenarioObjectives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioId = table.Column<int>(type: "int", nullable: false),
                    HexId = table.Column<int>(type: "int", nullable: false),
                    SideId = table.Column<int>(type: "int", nullable: false),
                    VictoryPoints = table.Column<int>(type: "int", nullable: false),
                    ConditionKind = table.Column<int>(type: "int", nullable: false),
                    TurnThreshold = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioObjectives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioObjectives_Hexes_HexId",
                        column: x => x.HexId,
                        principalTable: "Hexes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScenarioObjectives_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScenarioObjectives_Sides_SideId",
                        column: x => x.SideId,
                        principalTable: "Sides",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TokenPieces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TokenDesignId = table.Column<int>(type: "int", nullable: false),
                    SideId = table.Column<int>(type: "int", nullable: true),
                    Serial = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HardwareIdentity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsUnique = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenPieces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenPieces_Sides_SideId",
                        column: x => x.SideId,
                        principalTable: "Sides",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TokenPieces_TokenDesigns_TokenDesignId",
                        column: x => x.TokenDesignId,
                        principalTable: "TokenDesigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    StartHexId = table.Column<int>(type: "int", nullable: false),
                    Steps = table.Column<int>(type: "int", nullable: false),
                    Posture = table.Column<int>(type: "int", nullable: false),
                    Hidden = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioUnits_Hexes_StartHexId",
                        column: x => x.StartHexId,
                        principalTable: "Hexes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScenarioUnits_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScenarioUnits_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UnitCapabilities",
                columns: table => new
                {
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    AttackSoft = table.Column<int>(type: "int", nullable: false),
                    AttackHard = table.Column<int>(type: "int", nullable: false),
                    Defense = table.Column<int>(type: "int", nullable: false),
                    IndirectSupport = table.Column<int>(type: "int", nullable: false),
                    AtgmCount = table.Column<int>(type: "int", nullable: true),
                    MortarsCount = table.Column<int>(type: "int", nullable: true),
                    RocketsCount = table.Column<int>(type: "int", nullable: true),
                    HmgCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitCapabilities", x => x.UnitId);
                    table.ForeignKey(
                        name: "FK_UnitCapabilities_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TouchEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardCellId = table.Column<int>(type: "int", nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    Strength = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TouchEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TouchEvents_BoardCells_BoardCellId",
                        column: x => x.BoardCellId,
                        principalTable: "BoardCells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TouchEvents_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObjectiveControlLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    TurnId = table.Column<int>(type: "int", nullable: true),
                    ObjectiveId = table.Column<int>(type: "int", nullable: false),
                    SideId = table.Column<int>(type: "int", nullable: false),
                    GainedLost = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectiveControlLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObjectiveControlLogs_ScenarioObjectives_ObjectiveId",
                        column: x => x.ObjectiveId,
                        principalTable: "ScenarioObjectives",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ObjectiveControlLogs_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ObjectiveControlLogs_Sides_SideId",
                        column: x => x.SideId,
                        principalTable: "Sides",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ObjectiveControlLogs_Turns_TurnId",
                        column: x => x.TurnId,
                        principalTable: "Turns",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MoveEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    TurnId = table.Column<int>(type: "int", nullable: true),
                    TokenPieceId = table.Column<int>(type: "int", nullable: false),
                    FromHexId = table.Column<int>(type: "int", nullable: false),
                    ToHexId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoveEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoveEvents_Hexes_FromHexId",
                        column: x => x.FromHexId,
                        principalTable: "Hexes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MoveEvents_Hexes_ToHexId",
                        column: x => x.ToHexId,
                        principalTable: "Hexes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MoveEvents_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MoveEvents_TokenPieces_TokenPieceId",
                        column: x => x.TokenPieceId,
                        principalTable: "TokenPieces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MoveEvents_Turns_TurnId",
                        column: x => x.TurnId,
                        principalTable: "Turns",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Placements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    TurnId = table.Column<int>(type: "int", nullable: true),
                    TokenPieceId = table.Column<int>(type: "int", nullable: false),
                    HexId = table.Column<int>(type: "int", nullable: false),
                    PlacedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlacedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Placements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Placements_Hexes_HexId",
                        column: x => x.HexId,
                        principalTable: "Hexes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Placements_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Placements_TokenPieces_TokenPieceId",
                        column: x => x.TokenPieceId,
                        principalTable: "TokenPieces",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Placements_Turns_TurnId",
                        column: x => x.TurnId,
                        principalTable: "Turns",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SessionAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    TokenPieceId = table.Column<int>(type: "int", nullable: false),
                    ScenarioUnitId = table.Column<int>(type: "int", nullable: true),
                    HexFeatureId = table.Column<int>(type: "int", nullable: true),
                    ScenarioObjectiveId = table.Column<int>(type: "int", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionAssignments_HexFeatures_HexFeatureId",
                        column: x => x.HexFeatureId,
                        principalTable: "HexFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SessionAssignments_ScenarioObjectives_ScenarioObjectiveId",
                        column: x => x.ScenarioObjectiveId,
                        principalTable: "ScenarioObjectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SessionAssignments_ScenarioUnits_ScenarioUnitId",
                        column: x => x.ScenarioUnitId,
                        principalTable: "ScenarioUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SessionAssignments_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SessionAssignments_TokenPieces_TokenPieceId",
                        column: x => x.TokenPieceId,
                        principalTable: "TokenPieces",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardCells_BoardId",
                table: "BoardCells",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardCells_HexId",
                table: "BoardCells",
                column: "HexId");

            migrationBuilder.CreateIndex(
                name: "IX_Hexes_Q_R",
                table: "Hexes",
                columns: new[] { "Q", "R" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hexes_TerrainTypeId",
                table: "Hexes",
                column: "TerrainTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HexFeatures_FortificationTypeId",
                table: "HexFeatures",
                column: "FortificationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HexFeatures_HexId",
                table: "HexFeatures",
                column: "HexId");

            migrationBuilder.CreateIndex(
                name: "IX_HexFeatures_ObstacleTypeId",
                table: "HexFeatures",
                column: "ObstacleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HexFeatures_SideId",
                table: "HexFeatures",
                column: "SideId");

            migrationBuilder.CreateIndex(
                name: "IX_MoveEvents_FromHexId",
                table: "MoveEvents",
                column: "FromHexId");

            migrationBuilder.CreateIndex(
                name: "IX_MoveEvents_SessionId",
                table: "MoveEvents",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MoveEvents_ToHexId",
                table: "MoveEvents",
                column: "ToHexId");

            migrationBuilder.CreateIndex(
                name: "IX_MoveEvents_TokenPieceId",
                table: "MoveEvents",
                column: "TokenPieceId");

            migrationBuilder.CreateIndex(
                name: "IX_MoveEvents_TurnId",
                table: "MoveEvents",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectiveControlLogs_ObjectiveId",
                table: "ObjectiveControlLogs",
                column: "ObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectiveControlLogs_SessionId",
                table: "ObjectiveControlLogs",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectiveControlLogs_SideId",
                table: "ObjectiveControlLogs",
                column: "SideId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectiveControlLogs_TurnId",
                table: "ObjectiveControlLogs",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_Placements_HexId",
                table: "Placements",
                column: "HexId");

            migrationBuilder.CreateIndex(
                name: "IX_Placements_SessionId",
                table: "Placements",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Placements_TokenPieceId",
                table: "Placements",
                column: "TokenPieceId");

            migrationBuilder.CreateIndex(
                name: "IX_Placements_TurnId",
                table: "Placements",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioObjectives_HexId",
                table: "ScenarioObjectives",
                column: "HexId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioObjectives_ScenarioId",
                table: "ScenarioObjectives",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioObjectives_SideId",
                table: "ScenarioObjectives",
                column: "SideId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioUnits_ScenarioId",
                table: "ScenarioUnits",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioUnits_StartHexId",
                table: "ScenarioUnits",
                column: "StartHexId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioUnits_UnitId",
                table: "ScenarioUnits",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAssignments_HexFeatureId",
                table: "SessionAssignments",
                column: "HexFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAssignments_ScenarioObjectiveId",
                table: "SessionAssignments",
                column: "ScenarioObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAssignments_ScenarioUnitId",
                table: "SessionAssignments",
                column: "ScenarioUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAssignments_SessionId",
                table: "SessionAssignments",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAssignments_TokenPieceId",
                table: "SessionAssignments",
                column: "TokenPieceId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_CurrentSideId",
                table: "Sessions",
                column: "CurrentSideId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ScenarioId",
                table: "Sessions",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Sides_Name",
                table: "Sides",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TerrainTypes_Name",
                table: "TerrainTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenDesigns_DefaultSideId",
                table: "TokenDesigns",
                column: "DefaultSideId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenDesigns_TokenGroupId",
                table: "TokenDesigns",
                column: "TokenGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenPieces_HardwareIdentity",
                table: "TokenPieces",
                column: "HardwareIdentity",
                unique: true,
                filter: "[HardwareIdentity] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TokenPieces_SideId",
                table: "TokenPieces",
                column: "SideId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenPieces_TokenDesignId",
                table: "TokenPieces",
                column: "TokenDesignId");

            migrationBuilder.CreateIndex(
                name: "IX_TouchEvents_BoardCellId",
                table: "TouchEvents",
                column: "BoardCellId");

            migrationBuilder.CreateIndex(
                name: "IX_TouchEvents_SessionId",
                table: "TouchEvents",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_SessionId",
                table: "Turns",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_MovementProfileId",
                table: "Units",
                column: "MovementProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_SideId",
                table: "Units",
                column: "SideId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_UnitTypeId",
                table: "Units",
                column: "UnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitTypes_Name",
                table: "UnitTypes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MoveEvents");

            migrationBuilder.DropTable(
                name: "ObjectiveControlLogs");

            migrationBuilder.DropTable(
                name: "Placements");

            migrationBuilder.DropTable(
                name: "SessionAssignments");

            migrationBuilder.DropTable(
                name: "TouchEvents");

            migrationBuilder.DropTable(
                name: "UnitCapabilities");

            migrationBuilder.DropTable(
                name: "Turns");

            migrationBuilder.DropTable(
                name: "HexFeatures");

            migrationBuilder.DropTable(
                name: "ScenarioObjectives");

            migrationBuilder.DropTable(
                name: "ScenarioUnits");

            migrationBuilder.DropTable(
                name: "TokenPieces");

            migrationBuilder.DropTable(
                name: "BoardCells");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "FortificationTypes");

            migrationBuilder.DropTable(
                name: "ObstacleTypes");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "TokenDesigns");

            migrationBuilder.DropTable(
                name: "Boards");

            migrationBuilder.DropTable(
                name: "Hexes");

            migrationBuilder.DropTable(
                name: "Scenarios");

            migrationBuilder.DropTable(
                name: "MovementProfiles");

            migrationBuilder.DropTable(
                name: "UnitTypes");

            migrationBuilder.DropTable(
                name: "Sides");

            migrationBuilder.DropTable(
                name: "TokenGroups");

            migrationBuilder.DropTable(
                name: "TerrainTypes");
        }
    }
}
