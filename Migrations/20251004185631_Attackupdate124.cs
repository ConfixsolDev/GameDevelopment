using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class Attackupdate124 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FrontCoverageKm",
                table: "Tokens",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RearCoverageKm",
                table: "Tokens",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SideCoverageKm",
                table: "Tokens",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FrontRadiusKm",
                table: "TokenAreaCoverages",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RearRadiusKm",
                table: "TokenAreaCoverages",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RotationDegrees",
                table: "TokenAreaCoverages",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SideRadiusKm",
                table: "TokenAreaCoverages",
                type: "decimal(8,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FrontCoverageKm",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "RearCoverageKm",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "SideCoverageKm",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "FrontRadiusKm",
                table: "TokenAreaCoverages");

            migrationBuilder.DropColumn(
                name: "RearRadiusKm",
                table: "TokenAreaCoverages");

            migrationBuilder.DropColumn(
                name: "RotationDegrees",
                table: "TokenAreaCoverages");

            migrationBuilder.DropColumn(
                name: "SideRadiusKm",
                table: "TokenAreaCoverages");
        }
    }
}
