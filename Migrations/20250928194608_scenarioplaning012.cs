using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class scenarioplaning012 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CombatPower",
                table: "UnitDeployments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "CurrentTerrain",
                table: "UnitDeployments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "EffectiveCombatPower",
                table: "UnitDeployments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MovementPoints",
                table: "UnitDeployments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RemainingMovement",
                table: "UnitDeployments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StrengthPercentage",
                table: "UnitDeployments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SupplyModifier",
                table: "UnitDeployments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "SupplyState",
                table: "UnitDeployments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "TerrainModifier",
                table: "UnitDeployments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CombatPower",
                table: "InfantryBattalions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "CurrentTerrain",
                table: "InfantryBattalions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "EffectiveCombatPower",
                table: "InfantryBattalions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MovementPoints",
                table: "InfantryBattalions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RemainingMovement",
                table: "InfantryBattalions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StrengthPercentage",
                table: "InfantryBattalions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SupplyModifier",
                table: "InfantryBattalions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "SupplyState",
                table: "InfantryBattalions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "TerrainModifier",
                table: "InfantryBattalions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CombatPower",
                table: "ArtilleryRegiments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "CurrentTerrain",
                table: "ArtilleryRegiments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "EffectiveCombatPower",
                table: "ArtilleryRegiments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MovementPoints",
                table: "ArtilleryRegiments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RemainingMovement",
                table: "ArtilleryRegiments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StrengthPercentage",
                table: "ArtilleryRegiments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SupplyModifier",
                table: "ArtilleryRegiments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "SupplyState",
                table: "ArtilleryRegiments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "TerrainModifier",
                table: "ArtilleryRegiments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CombatPower",
                table: "ArmouredRegiments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "CurrentTerrain",
                table: "ArmouredRegiments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "EffectiveCombatPower",
                table: "ArmouredRegiments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MovementPoints",
                table: "ArmouredRegiments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RemainingMovement",
                table: "ArmouredRegiments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StrengthPercentage",
                table: "ArmouredRegiments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SupplyModifier",
                table: "ArmouredRegiments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "SupplyState",
                table: "ArmouredRegiments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "TerrainModifier",
                table: "ArmouredRegiments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CombatPower",
                table: "UnitDeployments");

            migrationBuilder.DropColumn(
                name: "CurrentTerrain",
                table: "UnitDeployments");

            migrationBuilder.DropColumn(
                name: "EffectiveCombatPower",
                table: "UnitDeployments");

            migrationBuilder.DropColumn(
                name: "MovementPoints",
                table: "UnitDeployments");

            migrationBuilder.DropColumn(
                name: "RemainingMovement",
                table: "UnitDeployments");

            migrationBuilder.DropColumn(
                name: "StrengthPercentage",
                table: "UnitDeployments");

            migrationBuilder.DropColumn(
                name: "SupplyModifier",
                table: "UnitDeployments");

            migrationBuilder.DropColumn(
                name: "SupplyState",
                table: "UnitDeployments");

            migrationBuilder.DropColumn(
                name: "TerrainModifier",
                table: "UnitDeployments");

            migrationBuilder.DropColumn(
                name: "CombatPower",
                table: "InfantryBattalions");

            migrationBuilder.DropColumn(
                name: "CurrentTerrain",
                table: "InfantryBattalions");

            migrationBuilder.DropColumn(
                name: "EffectiveCombatPower",
                table: "InfantryBattalions");

            migrationBuilder.DropColumn(
                name: "MovementPoints",
                table: "InfantryBattalions");

            migrationBuilder.DropColumn(
                name: "RemainingMovement",
                table: "InfantryBattalions");

            migrationBuilder.DropColumn(
                name: "StrengthPercentage",
                table: "InfantryBattalions");

            migrationBuilder.DropColumn(
                name: "SupplyModifier",
                table: "InfantryBattalions");

            migrationBuilder.DropColumn(
                name: "SupplyState",
                table: "InfantryBattalions");

            migrationBuilder.DropColumn(
                name: "TerrainModifier",
                table: "InfantryBattalions");

            migrationBuilder.DropColumn(
                name: "CombatPower",
                table: "ArtilleryRegiments");

            migrationBuilder.DropColumn(
                name: "CurrentTerrain",
                table: "ArtilleryRegiments");

            migrationBuilder.DropColumn(
                name: "EffectiveCombatPower",
                table: "ArtilleryRegiments");

            migrationBuilder.DropColumn(
                name: "MovementPoints",
                table: "ArtilleryRegiments");

            migrationBuilder.DropColumn(
                name: "RemainingMovement",
                table: "ArtilleryRegiments");

            migrationBuilder.DropColumn(
                name: "StrengthPercentage",
                table: "ArtilleryRegiments");

            migrationBuilder.DropColumn(
                name: "SupplyModifier",
                table: "ArtilleryRegiments");

            migrationBuilder.DropColumn(
                name: "SupplyState",
                table: "ArtilleryRegiments");

            migrationBuilder.DropColumn(
                name: "TerrainModifier",
                table: "ArtilleryRegiments");

            migrationBuilder.DropColumn(
                name: "CombatPower",
                table: "ArmouredRegiments");

            migrationBuilder.DropColumn(
                name: "CurrentTerrain",
                table: "ArmouredRegiments");

            migrationBuilder.DropColumn(
                name: "EffectiveCombatPower",
                table: "ArmouredRegiments");

            migrationBuilder.DropColumn(
                name: "MovementPoints",
                table: "ArmouredRegiments");

            migrationBuilder.DropColumn(
                name: "RemainingMovement",
                table: "ArmouredRegiments");

            migrationBuilder.DropColumn(
                name: "StrengthPercentage",
                table: "ArmouredRegiments");

            migrationBuilder.DropColumn(
                name: "SupplyModifier",
                table: "ArmouredRegiments");

            migrationBuilder.DropColumn(
                name: "SupplyState",
                table: "ArmouredRegiments");

            migrationBuilder.DropColumn(
                name: "TerrainModifier",
                table: "ArmouredRegiments");
        }
    }
}
