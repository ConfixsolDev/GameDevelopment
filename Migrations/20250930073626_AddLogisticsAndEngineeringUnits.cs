using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class AddLogisticsAndEngineeringUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CombatEngineeringCompanies");

            migrationBuilder.DropTable(
                name: "LogisticsUnits");
        }
    }
}
