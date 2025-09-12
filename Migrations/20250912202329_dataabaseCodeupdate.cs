using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class dataabaseCodeupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamTokenGroupAssignments_Teams_TeamId",
                table: "TeamTokenGroupAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tokens_TokenGroups_TokenGroupId",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_TokenSignatures_Timestamp",
                table: "TokenSignatures");

            migrationBuilder.DropIndex(
                name: "IX_Tokens_CreatedAt",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_Tokens_IsActive",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_Tokens_Name",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_TokenGroups_GroupCode",
                table: "TokenGroups");

            migrationBuilder.DropIndex(
                name: "IX_TokenGroups_IsActive",
                table: "TokenGroups");

            migrationBuilder.DropIndex(
                name: "IX_TeamTokenGroupAssignments_IsActive",
                table: "TeamTokenGroupAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Teams_IsActive",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_TeamCode",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_MapMarkers_CreatedAt",
                table: "MapMarkers");

            migrationBuilder.DropIndex(
                name: "IX_MapMarkers_IsActive",
                table: "MapMarkers");

            migrationBuilder.DropIndex(
                name: "IX_GameSessions_SessionCode",
                table: "GameSessions");

            migrationBuilder.DropIndex(
                name: "IX_GameSessions_StartTime",
                table: "GameSessions");

            migrationBuilder.DropIndex(
                name: "IX_GameSessions_Status",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TokenGroups");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "TokenGroups");

            migrationBuilder.DropColumn(
                name: "CreatedByUserName",
                table: "TokenGroups");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TeamTokenGroupAssignments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "GameSessions");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "TouchPatterns",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "TokenSignatureId",
                table: "TouchPatterns",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "TokenSignatureId",
                table: "TouchGeometry",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "TokenId",
                table: "TokenSignatures",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "TokenSignatures",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TokenSignatures",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "TokenSignatures",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TokenSignatures",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TokenSignatures",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "TokenSignatures",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TokenSignatures",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "TokenSignatures",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Tokens",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Tokens",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "TokenGroups",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TeamId",
                table: "TeamTokenGroupAssignments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "Teams",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TokenSignatureId",
                table: "StabilityInfo",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "TokenSignatureId",
                table: "MultiTouchGeometry",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "TokenId",
                table: "MapMarkers",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "GameSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "GameSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "AppRoles",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AppRoles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "AppRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AppRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "AppRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AppRoles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "AppRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Brigades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BrigadeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ForceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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
                name: "ArmouredRegiments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Squadrons = table.Column<int>(type: "int", nullable: false),
                    Tanks = table.Column<int>(type: "int", nullable: false),
                    ATGMS = table.Column<int>(type: "int", nullable: false),
                    Mortars120mm = table.Column<int>(type: "int", nullable: false),
                    HMG = table.Column<int>(type: "int", nullable: false),
                    MarchingSpeedRoads = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarchingSpeedCrossCountry = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CombatAdvanceSpeed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BrigadeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UnitCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Strength = table.Column<int>(type: "int", nullable: false),
                    ForceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArmouredRegiments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArmouredRegiments_Brigades_BrigadeId",
                        column: x => x.BrigadeId,
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
                    BrigadeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UnitCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Strength = table.Column<int>(type: "int", nullable: false),
                    ForceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtilleryRegiments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtilleryRegiments_Brigades_BrigadeId",
                        column: x => x.BrigadeId,
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
                    MarchingSpeedTrucksRoads = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarchingSpeedAPCs = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarchingSpeedCrossCountry = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarchingSpeedAPCsCrossCountry = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CombatAdvanceSpeed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BrigadeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UnitCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Strength = table.Column<int>(type: "int", nullable: false),
                    ForceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfantryBattalions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InfantryBattalions_Brigades_BrigadeId",
                        column: x => x.BrigadeId,
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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
                name: "UnitDeployments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScenarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ForceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Formation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CurrentStrength = table.Column<int>(type: "int", nullable: false),
                    MaxStrength = table.Column<int>(type: "int", nullable: false),
                    Morale = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Fatigue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitDeployments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitDeployments_WarGameScenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "WarGameScenarios",
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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
                        name: "FK_BattleParticipants_UnitDeployments_UnitDeploymentId",
                        column: x => x.UnitDeploymentId,
                        principalTable: "UnitDeployments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    MovementType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovementOrders_UnitDeployments_UnitDeploymentId",
                        column: x => x.UnitDeploymentId,
                        principalTable: "UnitDeployments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovementOrders_WarGameScenarios_WarGameScenarioId",
                        column: x => x.WarGameScenarioId,
                        principalTable: "WarGameScenarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArmouredRegiments_BrigadeId",
                table: "ArmouredRegiments",
                column: "BrigadeId");

            migrationBuilder.CreateIndex(
                name: "IX_ArmouredRegiments_TeamId",
                table: "ArmouredRegiments",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtilleryRegiments_BrigadeId",
                table: "ArtilleryRegiments",
                column: "BrigadeId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtilleryRegiments_TeamId",
                table: "ArtilleryRegiments",
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
                name: "IX_CombatResults_BattleId",
                table: "CombatResults",
                column: "BattleId");

            migrationBuilder.CreateIndex(
                name: "IX_ForceProtections_TeamId",
                table: "ForceProtections",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_InfantryBattalions_BrigadeId",
                table: "InfantryBattalions",
                column: "BrigadeId");

            migrationBuilder.CreateIndex(
                name: "IX_InfantryBattalions_TeamId",
                table: "InfantryBattalions",
                column: "TeamId");

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
                name: "IX_SimulationEvents_ScenarioId",
                table: "SimulationEvents",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrainMobilityFactors_TeamId",
                table: "TerrainMobilityFactors",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitDeployments_ScenarioId",
                table: "UnitDeployments",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_WarGameScenarios_GameSessionId",
                table: "WarGameScenarios",
                column: "GameSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamTokenGroupAssignments_Teams_TeamId",
                table: "TeamTokenGroupAssignments",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tokens_TokenGroups_TokenGroupId",
                table: "Tokens",
                column: "TokenGroupId",
                principalTable: "TokenGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamTokenGroupAssignments_Teams_TeamId",
                table: "TeamTokenGroupAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tokens_TokenGroups_TokenGroupId",
                table: "Tokens");

            migrationBuilder.DropTable(
                name: "ArmouredRegiments");

            migrationBuilder.DropTable(
                name: "ArtilleryRegiments");

            migrationBuilder.DropTable(
                name: "BattleParticipants");

            migrationBuilder.DropTable(
                name: "CombatResults");

            migrationBuilder.DropTable(
                name: "ForceProtections");

            migrationBuilder.DropTable(
                name: "InfantryBattalions");

            migrationBuilder.DropTable(
                name: "MovementOrders");

            migrationBuilder.DropTable(
                name: "Objectives");

            migrationBuilder.DropTable(
                name: "SimulationEvents");

            migrationBuilder.DropTable(
                name: "TerrainMobilityFactors");

            migrationBuilder.DropTable(
                name: "Battles");

            migrationBuilder.DropTable(
                name: "Brigades");

            migrationBuilder.DropTable(
                name: "UnitDeployments");

            migrationBuilder.DropTable(
                name: "WarGameScenarios");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TokenSignatures");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "TokenSignatures");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TokenSignatures");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TokenSignatures");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "TokenSignatures");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TokenSignatures");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "TokenSignatures");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "TokenGroups");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AppRoles");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "AppRoles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AppRoles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppRoles");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "AppRoles");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AppRoles");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "AppRoles");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "TouchPatterns",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TokenSignatureId",
                table: "TouchPatterns",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "TokenSignatureId",
                table: "TouchGeometry",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<long>(
                name: "TokenId",
                table: "TokenSignatures",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "TokenSignatures",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Tokens",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Tokens",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Tokens",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TokenGroups",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "TokenGroups",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserName",
                table: "TokenGroups",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TeamId",
                table: "TeamTokenGroupAssignments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TeamTokenGroupAssignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Teams",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<int>(
                name: "TokenSignatureId",
                table: "StabilityInfo",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "TokenSignatureId",
                table: "MultiTouchGeometry",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<long>(
                name: "TokenId",
                table: "MapMarkers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "GameSessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "AppRoles",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_TokenSignatures_Timestamp",
                table: "TokenSignatures",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_CreatedAt",
                table: "Tokens",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_IsActive",
                table: "Tokens",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_Name",
                table: "Tokens",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenGroups_GroupCode",
                table: "TokenGroups",
                column: "GroupCode");

            migrationBuilder.CreateIndex(
                name: "IX_TokenGroups_IsActive",
                table: "TokenGroups",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TeamTokenGroupAssignments_IsActive",
                table: "TeamTokenGroupAssignments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_IsActive",
                table: "Teams",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_TeamCode",
                table: "Teams",
                column: "TeamCode");

            migrationBuilder.CreateIndex(
                name: "IX_MapMarkers_CreatedAt",
                table: "MapMarkers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MapMarkers_IsActive",
                table: "MapMarkers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_SessionCode",
                table: "GameSessions",
                column: "SessionCode");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_StartTime",
                table: "GameSessions",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_Status",
                table: "GameSessions",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamTokenGroupAssignments_Teams_TeamId",
                table: "TeamTokenGroupAssignments",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tokens_TokenGroups_TokenGroupId",
                table: "Tokens",
                column: "TokenGroupId",
                principalTable: "TokenGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
