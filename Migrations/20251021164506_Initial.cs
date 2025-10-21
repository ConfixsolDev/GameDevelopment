using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoleAccess = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Access = table.Column<string>(type: "nvarchar(max)", maxLength: 4000, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttackIntents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttackPreparation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NatoAttackType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttackIntensity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoordinationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DesiredEffect = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttackIntents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttackLogistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplyThreshold = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinimumSupplyRequired = table.Column<int>(type: "int", nullable: false),
                    ExpectedSupplyConsumption = table.Column<int>(type: "int", nullable: false),
                    ResupplyAvailable = table.Column<bool>(type: "bit", nullable: false),
                    ResupplyTurn = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttackLogistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttackMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MpReservePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttackMovements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttackTimings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTurn = table.Column<int>(type: "int", nullable: false),
                    DurationTurns = table.Column<int>(type: "int", nullable: false),
                    Posture = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttackTimings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnhancedAttackOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttackerTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IntentJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FiresJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MovementJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimingJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FogOfWarJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogisticsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ROEJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ExecutionMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExecutedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnhancedAttackOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FiresSupports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArtilleryAttached = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArtilleryTask = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EngineersPresent = table.Column<bool>(type: "bit", nullable: false),
                    EngineerUnits = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiresSupports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FogOfWars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DetectionConfidence = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommitWithUncertainty = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FogOfWars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SessionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MapConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfigurationType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MapDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RegionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObstaclesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SafeJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MapLabels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    LabelType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FontSize = table.Column<int>(type: "int", nullable: true),
                    FontWeight = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapLabels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MapRegions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Geometry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreaM2 = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    CenterLat = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    CenterLng = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RegionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapRegions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MapSectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Geometry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LandType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreaM2 = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    CenterLat = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    CenterLng = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SectorType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapSectors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RulesOfEngagements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollateralSensitivity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CivilianPopulationPresent = table.Column<bool>(type: "bit", nullable: false),
                    CivilianPopulationDensity = table.Column<int>(type: "int", nullable: false),
                    InfrastructurePresent = table.Column<bool>(type: "bit", nullable: false),
                    InfrastructureTypes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WeaponRestrictions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RulesOfEngagements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TeamCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubTeamCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ForceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedByUserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeamTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TeamTypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TokenGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GroupCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Waypoint",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DistanceToNext = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AttackMovementId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Waypoint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Waypoint_AttackMovements_AttackMovementId",
                        column: x => x.AttackMovementId,
                        principalTable: "AttackMovements",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AbortCriteria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FogOfWarId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbortCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbortCriteria_FogOfWars_FogOfWarId",
                        column: x => x.FogOfWarId,
                        principalTable: "FogOfWars",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WarGameScenarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ScenarioCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GameSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarGameScenarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarGameScenarios_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LoginCount = table.Column<int>(type: "int", nullable: false),
                    UserCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ForceType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HomeUrl = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    isSuperAdmin = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ForceProtections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ForceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProtectionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DegreeOfPreparation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProtectionFactor = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForceProtections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForceProtections_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TerrainMobilityFactors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TerrainType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    XFactor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TankPSIMin = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TankPSIMax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    APCPsimMin = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    APCPsimMax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerrainMobilityFactors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TerrainMobilityFactors_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TerrainTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TerrainCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MovementCostRoad = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MovementCostCrossCountry = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CombatModifier = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPassable = table.Column<bool>(type: "bit", nullable: false),
                    IsImpassableToVehicles = table.Column<bool>(type: "bit", nullable: false),
                    VisibilityModifier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerrainTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TerrainTypes_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TeamTokenGroupAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AssignedByUserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamTokenGroupAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamTokenGroupAssignments_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamTokenGroupAssignments_TokenGroups_TokenGroupId",
                        column: x => x.TokenGroupId,
                        principalTable: "TokenGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TrainingConsistency = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsManualToken = table.Column<bool>(type: "bit", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ForceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TokenGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssetImagePath = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FrontCoverageKm = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    RearCoverageKm = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    SideCoverageKm = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    OrganizationLevel = table.Column<int>(type: "int", nullable: true),
                    UnitType = table.Column<int>(type: "int", nullable: true),
                    UnitDesignation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tokens_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tokens_TokenGroups_TokenGroupId",
                        column: x => x.TokenGroupId,
                        principalTable: "TokenGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Battles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BattleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ScenarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BattleLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BattleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Victor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TerrainType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TerrainModifier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WeatherConditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WeatherModifier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BattleResults = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BattleLog = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Battles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Battles_WarGameScenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "WarGameScenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Objectives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObjectiveName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ScenarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObjectiveLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ObjectiveType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedToForce = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PointValue = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedByTeam = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Objectives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Objectives_WarGameScenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "WarGameScenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SimulationEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScenarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EventData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TriggeredByUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TriggeredByUserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AffectedTeamId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulationEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SimulationEvents_WarGameScenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "WarGameScenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnitDeployment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScenarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ForceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Formation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CurrentStrength = table.Column<int>(type: "int", nullable: false),
                    MaxStrength = table.Column<int>(type: "int", nullable: false),
                    Morale = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Fatigue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MovementPointsPerTurn = table.Column<int>(type: "int", nullable: false),
                    CurrentTerrain = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    RemainingMovementPoints = table.Column<int>(type: "int", nullable: false),
                    MovementPoints = table.Column<double>(type: "float", nullable: false),
                    RemainingMovement = table.Column<double>(type: "float", nullable: false),
                    CombatPowerIndex = table.Column<double>(type: "float(18)", precision: 18, scale: 2, nullable: false),
                    TerrainModifier = table.Column<double>(type: "float(18)", precision: 18, scale: 2, nullable: false),
                    SupplyModifier = table.Column<double>(type: "float(18)", precision: 18, scale: 2, nullable: false),
                    CombatPower = table.Column<double>(type: "float", nullable: false),
                    EffectiveCombatPower = table.Column<double>(type: "float", nullable: false),
                    StrengthPercentage = table.Column<double>(type: "float(18)", precision: 18, scale: 2, nullable: false),
                    EffectiveCombatPower_RO = table.Column<double>(type: "float(18)", precision: 18, scale: 2, nullable: false),
                    SupplyState = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    SupplyStateInt = table.Column<int>(type: "int", nullable: false),
                    DetectionProbability = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitDeployment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitDeployment_WarGameScenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "WarGameScenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttackOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttackerTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AxisId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ArtilleryAttached = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MpReservePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Posture = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExpectedStartTurn = table.Column<int>(type: "int", nullable: false),
                    DurationTurns = table.Column<int>(type: "int", nullable: false),
                    ExecutionMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExecutedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PayloadJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DetectionConfidence = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsLowConfidence = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttackOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttackOrders_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AttackOrders_Tokens_AttackerTokenId",
                        column: x => x.AttackerTokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttackOrders_Tokens_TargetTokenId",
                        column: x => x.TargetTokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Brigades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrigadeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ForceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brigades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Brigades_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Brigades_Tokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DefenseElements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ElementId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Coordinates = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Strength = table.Column<int>(type: "int", nullable: false),
                    Effectiveness = table.Column<double>(type: "float", nullable: false),
                    Visibility = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefenseElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DefenseElements_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DefenseElements_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DefenseElements_Tokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Intelligence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Intelligence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Intelligence_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Intelligence_Tokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MapMarkers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenId_GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    latitude = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    longitude = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    ZIndex = table.Column<int>(type: "int", nullable: false),
                    MarkerType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapMarkers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MapMarkers_Tokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SuspectedTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PlacerSide = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(10,7)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(10,7)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Confidence = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FirstDetectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SuspectedType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MarkerStyle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VisibleTo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RealTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PositionAccuracyMeters = table.Column<int>(type: "int", nullable: true),
                    MatchingConfidence = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuspectedTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuspectedTokens_Tokens_RealTokenId",
                        column: x => x.RealTokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TokenAreaCoverages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Geometry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AreaKm2 = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    FrontRadiusKm = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    RearRadiusKm = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    SideRadiusKm = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    RotationDegrees = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    CoverageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShapeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsDynamic = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenAreaCoverages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenAreaCoverages_Tokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TokenSignatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TouchCount = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OriginalTouches = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Distances = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Angles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Center = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenSignatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenSignatures_Tokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CombatResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BattleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttackerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CombatType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AttackerStrength = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DefenderStrength = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AttackerLosses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DefenderLosses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TerrainModifier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProtectionModifier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CombatTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CombatDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombatResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CombatResults_Battles_BattleId",
                        column: x => x.BattleId,
                        principalTable: "Battles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BattleParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BattleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitDeploymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InitialStrength = table.Column<int>(type: "int", nullable: false),
                    FinalStrength = table.Column<int>(type: "int", nullable: false),
                    Casualties = table.Column<int>(type: "int", nullable: false),
                    CombatEffectiveness = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProtectionFactor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProtectionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Equipment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattleParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BattleParticipants_Battles_BattleId",
                        column: x => x.BattleId,
                        principalTable: "Battles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BattleParticipants_UnitDeployment_UnitDeploymentId",
                        column: x => x.UnitDeploymentId,
                        principalTable: "UnitDeployment",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MovementOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitDeploymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartPosition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndPosition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Waypoints = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MovementType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EngagementRule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedArrival = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualArrival = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Speed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Distance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TerrainFactors = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IssuedByUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IssuedByUserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WarGameScenarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovementOrders_UnitDeployment_UnitDeploymentId",
                        column: x => x.UnitDeploymentId,
                        principalTable: "UnitDeployment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovementOrders_WarGameScenarios_WarGameScenarioId",
                        column: x => x.WarGameScenarioId,
                        principalTable: "WarGameScenarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoutesDrafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RouteName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WaypointsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCommitted = table.Column<bool>(type: "bit", nullable: false),
                    TotalDistanceKm = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EstimatedTimeTurns = table.Column<int>(type: "int", nullable: false),
                    SupplyImpact = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RouteType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedByUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommittedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutesDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutesDrafts_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RoutesDrafts_UnitDeployment_UnitId",
                        column: x => x.UnitId,
                        principalTable: "UnitDeployment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ArmouredRegiments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Squadrons = table.Column<int>(type: "int", nullable: false),
                    Tanks = table.Column<int>(type: "int", nullable: false),
                    ATGMS = table.Column<int>(type: "int", nullable: false),
                    Mortars120mm = table.Column<int>(type: "int", nullable: false),
                    HMG = table.Column<int>(type: "int", nullable: false),
                    Drones = table.Column<int>(type: "int", nullable: false),
                    DroneTypes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MarchingSpeedRoads = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarchingSpeedCrossCountry = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CombatAdvanceSpeed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BrigadeId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UnitCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Strength = table.Column<int>(type: "int", nullable: false),
                    ForceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BrigadeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MovementPoints = table.Column<double>(type: "float", nullable: false),
                    CurrentTerrain = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RemainingMovement = table.Column<double>(type: "float", nullable: false),
                    CombatPower = table.Column<double>(type: "float", nullable: false),
                    TerrainModifier = table.Column<double>(type: "float", nullable: false),
                    SupplyModifier = table.Column<double>(type: "float", nullable: false),
                    StrengthPercentage = table.Column<double>(type: "float", nullable: false),
                    EffectiveCombatPower = table.Column<double>(type: "float", nullable: false),
                    SupplyState = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArmouredRegiments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArmouredRegiments_Brigades_BrigadeId",
                        column: x => x.BrigadeId,
                        principalTable: "Brigades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ArmouredRegiments_Brigades_BrigadeId1",
                        column: x => x.BrigadeId1,
                        principalTable: "Brigades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ArmouredRegiments_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ArtilleryRegiments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Batteries = table.Column<int>(type: "int", nullable: false),
                    Guns = table.Column<int>(type: "int", nullable: false),
                    GunRange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HMG = table.Column<int>(type: "int", nullable: false),
                    GunCaliber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Drones = table.Column<int>(type: "int", nullable: false),
                    DroneTypes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BrigadeId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UnitCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Strength = table.Column<int>(type: "int", nullable: false),
                    ForceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BrigadeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MovementPoints = table.Column<double>(type: "float", nullable: false),
                    CurrentTerrain = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RemainingMovement = table.Column<double>(type: "float", nullable: false),
                    CombatPower = table.Column<double>(type: "float", nullable: false),
                    TerrainModifier = table.Column<double>(type: "float", nullable: false),
                    SupplyModifier = table.Column<double>(type: "float", nullable: false),
                    StrengthPercentage = table.Column<double>(type: "float", nullable: false),
                    EffectiveCombatPower = table.Column<double>(type: "float", nullable: false),
                    SupplyState = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtilleryRegiments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtilleryRegiments_Brigades_BrigadeId",
                        column: x => x.BrigadeId,
                        principalTable: "Brigades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ArtilleryRegiments_Brigades_BrigadeId1",
                        column: x => x.BrigadeId1,
                        principalTable: "Brigades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ArtilleryRegiments_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CombatEngineeringCompanies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Platoons = table.Column<int>(type: "int", nullable: false),
                    EngineerVehicles = table.Column<int>(type: "int", nullable: false),
                    BridgeLayingVehicles = table.Column<int>(type: "int", nullable: false),
                    MineClearingVehicles = table.Column<int>(type: "int", nullable: false),
                    Bulldozers = table.Column<int>(type: "int", nullable: false),
                    Excavators = table.Column<int>(type: "int", nullable: false),
                    Cranes = table.Column<int>(type: "int", nullable: false),
                    DemolitionCharges = table.Column<int>(type: "int", nullable: false),
                    MineDetectionEquipment = table.Column<int>(type: "int", nullable: false),
                    ConstructionMaterials = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HMG = table.Column<int>(type: "int", nullable: false),
                    LMG = table.Column<int>(type: "int", nullable: false),
                    ATGMS = table.Column<int>(type: "int", nullable: false),
                    BridgeBuildingCapacity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FortificationBuildingCapacity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ObstacleClearingCapacity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MarchingSpeedRoads = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarchingSpeedCrossCountry = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EngineeringWorkSpeed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BrigadeId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UnitCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Strength = table.Column<int>(type: "int", nullable: false),
                    ForceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BrigadeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MovementPoints = table.Column<double>(type: "float", nullable: false),
                    CurrentTerrain = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RemainingMovement = table.Column<double>(type: "float", nullable: false),
                    CombatPower = table.Column<double>(type: "float", nullable: false),
                    TerrainModifier = table.Column<double>(type: "float", nullable: false),
                    SupplyModifier = table.Column<double>(type: "float", nullable: false),
                    StrengthPercentage = table.Column<double>(type: "float", nullable: false),
                    EffectiveCombatPower = table.Column<double>(type: "float", nullable: false),
                    SupplyState = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombatEngineeringCompanies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CombatEngineeringCompanies_Brigades_BrigadeId",
                        column: x => x.BrigadeId,
                        principalTable: "Brigades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CombatEngineeringCompanies_Brigades_BrigadeId1",
                        column: x => x.BrigadeId1,
                        principalTable: "Brigades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CombatEngineeringCompanies_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InfantryBattalions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Companies = table.Column<int>(type: "int", nullable: false),
                    ATGMS = table.Column<int>(type: "int", nullable: false),
                    RocketLauncher = table.Column<int>(type: "int", nullable: false),
                    Mortars81mm = table.Column<int>(type: "int", nullable: false),
                    Mortars120mm = table.Column<int>(type: "int", nullable: false),
                    GrenadeLaunchers = table.Column<int>(type: "int", nullable: false),
                    HMG_AGL = table.Column<int>(type: "int", nullable: false),
                    MG_LMG = table.Column<int>(type: "int", nullable: false),
                    MANPADS = table.Column<int>(type: "int", nullable: false),
                    Grenades = table.Column<int>(type: "int", nullable: false),
                    Drones = table.Column<int>(type: "int", nullable: false),
                    DroneTypes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MarchingSpeedTrucksRoads = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarchingSpeedAPCs = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarchingSpeedCrossCountry = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarchingSpeedAPCsCrossCountry = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CombatAdvanceSpeed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BrigadeId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UnitCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Strength = table.Column<int>(type: "int", nullable: false),
                    ForceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BrigadeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MovementPoints = table.Column<double>(type: "float", nullable: false),
                    CurrentTerrain = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RemainingMovement = table.Column<double>(type: "float", nullable: false),
                    CombatPower = table.Column<double>(type: "float", nullable: false),
                    TerrainModifier = table.Column<double>(type: "float", nullable: false),
                    SupplyModifier = table.Column<double>(type: "float", nullable: false),
                    StrengthPercentage = table.Column<double>(type: "float", nullable: false),
                    EffectiveCombatPower = table.Column<double>(type: "float", nullable: false),
                    SupplyState = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfantryBattalions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InfantryBattalions_Brigades_BrigadeId",
                        column: x => x.BrigadeId,
                        principalTable: "Brigades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InfantryBattalions_Brigades_BrigadeId1",
                        column: x => x.BrigadeId1,
                        principalTable: "Brigades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InfantryBattalions_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Companies = table.Column<int>(type: "int", nullable: false),
                    SupplyTrucks = table.Column<int>(type: "int", nullable: false),
                    FuelTrucks = table.Column<int>(type: "int", nullable: false),
                    WaterTrucks = table.Column<int>(type: "int", nullable: false),
                    AmmunitionTrucks = table.Column<int>(type: "int", nullable: false),
                    MaintenanceVehicles = table.Column<int>(type: "int", nullable: false),
                    RecoveryVehicles = table.Column<int>(type: "int", nullable: false),
                    MobileWorkshops = table.Column<int>(type: "int", nullable: false),
                    FuelCapacity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WaterCapacity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmmunitionStorage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SupplyCapacity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HMG = table.Column<int>(type: "int", nullable: false),
                    LMG = table.Column<int>(type: "int", nullable: false),
                    MarchingSpeedRoads = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarchingSpeedCrossCountry = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConvoySpeed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BrigadeId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UnitCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Strength = table.Column<int>(type: "int", nullable: false),
                    ForceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BrigadeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MovementPoints = table.Column<double>(type: "float", nullable: false),
                    CurrentTerrain = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RemainingMovement = table.Column<double>(type: "float", nullable: false),
                    CombatPower = table.Column<double>(type: "float", nullable: false),
                    TerrainModifier = table.Column<double>(type: "float", nullable: false),
                    SupplyModifier = table.Column<double>(type: "float", nullable: false),
                    StrengthPercentage = table.Column<double>(type: "float", nullable: false),
                    EffectiveCombatPower = table.Column<double>(type: "float", nullable: false),
                    SupplyState = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogisticsUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogisticsUnits_Brigades_BrigadeId",
                        column: x => x.BrigadeId,
                        principalTable: "Brigades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LogisticsUnits_Brigades_BrigadeId1",
                        column: x => x.BrigadeId1,
                        principalTable: "Brigades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LogisticsUnits_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Recon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReconType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Confidence = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BrigadeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recon_Brigades_BrigadeId",
                        column: x => x.BrigadeId,
                        principalTable: "Brigades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Recon_Tokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ISRMissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SuspectedTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssetType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfidenceGain = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CostFuel = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    ExposureRisk = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    RequestedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Results = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ISRMissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ISRMissions_SuspectedTokens_SuspectedTokenId",
                        column: x => x.SuspectedTokenId,
                        principalTable: "SuspectedTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MultiTouchGeometry",
                columns: table => new
                {
                    TokenSignatureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AspectRatio = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    BoundingBoxWidth = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    BoundingBoxHeight = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    BoundingBoxArea = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    CenterX = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    CenterY = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    Spread = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    Density = table.Column<decimal>(type: "decimal(10,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiTouchGeometry", x => x.TokenSignatureId);
                    table.ForeignKey(
                        name: "FK_MultiTouchGeometry_TokenSignatures_TokenSignatureId",
                        column: x => x.TokenSignatureId,
                        principalTable: "TokenSignatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StabilityInfo",
                columns: table => new
                {
                    TokenSignatureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsStabilized = table.Column<bool>(type: "bit", nullable: false),
                    GeneratedAt = table.Column<long>(type: "bigint", nullable: false),
                    SampleCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StabilityInfo", x => x.TokenSignatureId);
                    table.ForeignKey(
                        name: "FK_StabilityInfo_TokenSignatures_TokenSignatureId",
                        column: x => x.TokenSignatureId,
                        principalTable: "TokenSignatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TouchGeometry",
                columns: table => new
                {
                    TokenSignatureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HasRadius = table.Column<bool>(type: "bit", nullable: false),
                    HasRotation = table.Column<bool>(type: "bit", nullable: false),
                    RadiusValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RotationValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AvgRadius = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    AvgRotation = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    RadiusVariance = table.Column<decimal>(type: "decimal(10,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TouchGeometry", x => x.TokenSignatureId);
                    table.ForeignKey(
                        name: "FK_TouchGeometry_TokenSignatures_TokenSignatureId",
                        column: x => x.TokenSignatureId,
                        principalTable: "TokenSignatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TouchPatterns",
                columns: table => new
                {
                    TokenSignatureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Complexity = table.Column<int>(type: "int", nullable: false),
                    Distances = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistancePairs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AvgDistance = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    MinDistance = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    MaxDistance = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    DistanceRange = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    DistanceVariance = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    DistanceSignature = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AngleSpread = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    GeometricCenter = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TouchPatterns", x => x.TokenSignatureId);
                    table.ForeignKey(
                        name: "FK_TouchPatterns_TokenSignatures_TokenSignatureId",
                        column: x => x.TokenSignatureId,
                        principalTable: "TokenSignatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbortCriteria_FogOfWarId",
                table: "AbortCriteria",
                column: "FogOfWarId");

            migrationBuilder.CreateIndex(
                name: "IX_Armoured_Token_Brigade_Team_Active",
                table: "ArmouredRegiments",
                columns: new[] { "TokenId", "BrigadeId", "TeamId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ArmouredRegiments_BrigadeId",
                table: "ArmouredRegiments",
                column: "BrigadeId");

            migrationBuilder.CreateIndex(
                name: "IX_ArmouredRegiments_BrigadeId1",
                table: "ArmouredRegiments",
                column: "BrigadeId1");

            migrationBuilder.CreateIndex(
                name: "IX_ArmouredRegiments_TeamId",
                table: "ArmouredRegiments",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Artillery_Token_Brigade_Team_Active",
                table: "ArtilleryRegiments",
                columns: new[] { "TokenId", "BrigadeId", "TeamId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ArtilleryRegiments_BrigadeId",
                table: "ArtilleryRegiments",
                column: "BrigadeId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtilleryRegiments_BrigadeId1",
                table: "ArtilleryRegiments",
                column: "BrigadeId1");

            migrationBuilder.CreateIndex(
                name: "IX_ArtilleryRegiments_TeamId",
                table: "ArtilleryRegiments",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TeamId",
                table: "AspNetUsers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AttackOrders_AttackerTokenId",
                table: "AttackOrders",
                column: "AttackerTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_AttackOrders_ExpectedStartTurn",
                table: "AttackOrders",
                column: "ExpectedStartTurn");

            migrationBuilder.CreateIndex(
                name: "IX_AttackOrders_Status",
                table: "AttackOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AttackOrders_TargetTokenId",
                table: "AttackOrders",
                column: "TargetTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_AttackOrders_TeamId",
                table: "AttackOrders",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleParticipants_BattleId",
                table: "BattleParticipants",
                column: "BattleId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleParticipants_UnitDeploymentId",
                table: "BattleParticipants",
                column: "UnitDeploymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Battles_ScenarioId",
                table: "Battles",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Brigades_TeamId",
                table: "Brigades",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Brigades_TokenId",
                table: "Brigades",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_CombatEngineeringCompanies_BrigadeId",
                table: "CombatEngineeringCompanies",
                column: "BrigadeId");

            migrationBuilder.CreateIndex(
                name: "IX_CombatEngineeringCompanies_BrigadeId1",
                table: "CombatEngineeringCompanies",
                column: "BrigadeId1");

            migrationBuilder.CreateIndex(
                name: "IX_CombatEngineeringCompanies_TeamId",
                table: "CombatEngineeringCompanies",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Engineering_Token_Brigade_Team_Active",
                table: "CombatEngineeringCompanies",
                columns: new[] { "TokenId", "BrigadeId", "TeamId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CombatResults_BattleId",
                table: "CombatResults",
                column: "BattleId");

            migrationBuilder.CreateIndex(
                name: "IX_DefenseElement_Session_Status",
                table: "DefenseElements",
                columns: new[] { "GameSessionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DefenseElement_Team_Status_Date",
                table: "DefenseElements",
                columns: new[] { "TeamId", "Status", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DefenseElements_Category",
                table: "DefenseElements",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_DefenseElements_ElementId",
                table: "DefenseElements",
                column: "ElementId");

            migrationBuilder.CreateIndex(
                name: "IX_DefenseElements_GameSessionId",
                table: "DefenseElements",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_DefenseElements_Status",
                table: "DefenseElements",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DefenseElements_TokenId",
                table: "DefenseElements",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_ForceProtections_TeamId",
                table: "ForceProtections",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Infantry_Token_Brigade_Team_Active",
                table: "InfantryBattalions",
                columns: new[] { "TokenId", "BrigadeId", "TeamId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_InfantryBattalions_BrigadeId",
                table: "InfantryBattalions",
                column: "BrigadeId");

            migrationBuilder.CreateIndex(
                name: "IX_InfantryBattalions_BrigadeId1",
                table: "InfantryBattalions",
                column: "BrigadeId1");

            migrationBuilder.CreateIndex(
                name: "IX_InfantryBattalions_TeamId",
                table: "InfantryBattalions",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Intelligence_TeamId",
                table: "Intelligence",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Intelligence_TokenId",
                table: "Intelligence",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_ISRMissions_SuspectedTokenId",
                table: "ISRMissions",
                column: "SuspectedTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_Logistics_Token_Brigade_Team_Active",
                table: "LogisticsUnits",
                columns: new[] { "TokenId", "BrigadeId", "TeamId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsUnits_BrigadeId",
                table: "LogisticsUnits",
                column: "BrigadeId");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsUnits_BrigadeId1",
                table: "LogisticsUnits",
                column: "BrigadeId1");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsUnits_TeamId",
                table: "LogisticsUnits",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MapMarkers_TokenId",
                table: "MapMarkers",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementOrders_UnitDeploymentId",
                table: "MovementOrders",
                column: "UnitDeploymentId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementOrders_WarGameScenarioId",
                table: "MovementOrders",
                column: "WarGameScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_ScenarioId",
                table: "Objectives",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Recon_BrigadeId",
                table: "Recon",
                column: "BrigadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Recon_TokenId",
                table: "Recon",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutesDrafts_TeamId",
                table: "RoutesDrafts",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutesDrafts_UnitId",
                table: "RoutesDrafts",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SimulationEvents_ScenarioId",
                table: "SimulationEvents",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SuspectedTokens_RealTokenId",
                table: "SuspectedTokens",
                column: "RealTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamTokenGroupAssignments_TeamId",
                table: "TeamTokenGroupAssignments",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamTokenGroupAssignments_TokenGroupId",
                table: "TeamTokenGroupAssignments",
                column: "TokenGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrainMobilityFactors_TeamId",
                table: "TerrainMobilityFactors",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrainTypes_TeamId",
                table: "TerrainTypes",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenAreaCoverages_TokenId",
                table: "TokenAreaCoverages",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_TeamId",
                table: "Tokens",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_TokenGroupId",
                table: "Tokens",
                column: "TokenGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenSignatures_TokenId",
                table: "TokenSignatures",
                column: "TokenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitDeployment_ScenarioId",
                table: "UnitDeployment",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_WarGameScenarios_GameSessionId",
                table: "WarGameScenarios",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Waypoint_AttackMovementId",
                table: "Waypoint",
                column: "AttackMovementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbortCriteria");

            migrationBuilder.DropTable(
                name: "AppRoles");

            migrationBuilder.DropTable(
                name: "ArmouredRegiments");

            migrationBuilder.DropTable(
                name: "ArtilleryRegiments");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AttackIntents");

            migrationBuilder.DropTable(
                name: "AttackLogistics");

            migrationBuilder.DropTable(
                name: "AttackOrders");

            migrationBuilder.DropTable(
                name: "AttackTimings");

            migrationBuilder.DropTable(
                name: "BattleParticipants");

            migrationBuilder.DropTable(
                name: "CombatEngineeringCompanies");

            migrationBuilder.DropTable(
                name: "CombatResults");

            migrationBuilder.DropTable(
                name: "DefenseElements");

            migrationBuilder.DropTable(
                name: "EnhancedAttackOrders");

            migrationBuilder.DropTable(
                name: "FiresSupports");

            migrationBuilder.DropTable(
                name: "ForceProtections");

            migrationBuilder.DropTable(
                name: "InfantryBattalions");

            migrationBuilder.DropTable(
                name: "Intelligence");

            migrationBuilder.DropTable(
                name: "ISRMissions");

            migrationBuilder.DropTable(
                name: "LogisticsUnits");

            migrationBuilder.DropTable(
                name: "MapConfigurations");

            migrationBuilder.DropTable(
                name: "MapDocuments");

            migrationBuilder.DropTable(
                name: "MapLabels");

            migrationBuilder.DropTable(
                name: "MapMarkers");

            migrationBuilder.DropTable(
                name: "MapRegions");

            migrationBuilder.DropTable(
                name: "MapSectors");

            migrationBuilder.DropTable(
                name: "MovementOrders");

            migrationBuilder.DropTable(
                name: "MultiTouchGeometry");

            migrationBuilder.DropTable(
                name: "Objectives");

            migrationBuilder.DropTable(
                name: "Recon");

            migrationBuilder.DropTable(
                name: "RoutesDrafts");

            migrationBuilder.DropTable(
                name: "RulesOfEngagements");

            migrationBuilder.DropTable(
                name: "SimulationEvents");

            migrationBuilder.DropTable(
                name: "StabilityInfo");

            migrationBuilder.DropTable(
                name: "TeamTokenGroupAssignments");

            migrationBuilder.DropTable(
                name: "TeamTypes");

            migrationBuilder.DropTable(
                name: "TerrainMobilityFactors");

            migrationBuilder.DropTable(
                name: "TerrainTypes");

            migrationBuilder.DropTable(
                name: "TokenAreaCoverages");

            migrationBuilder.DropTable(
                name: "TouchGeometry");

            migrationBuilder.DropTable(
                name: "TouchPatterns");

            migrationBuilder.DropTable(
                name: "Waypoint");

            migrationBuilder.DropTable(
                name: "FogOfWars");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Battles");

            migrationBuilder.DropTable(
                name: "SuspectedTokens");

            migrationBuilder.DropTable(
                name: "Brigades");

            migrationBuilder.DropTable(
                name: "UnitDeployment");

            migrationBuilder.DropTable(
                name: "TokenSignatures");

            migrationBuilder.DropTable(
                name: "AttackMovements");

            migrationBuilder.DropTable(
                name: "WarGameScenarios");

            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "GameSessions");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "TokenGroups");
        }
    }
}
