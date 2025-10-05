using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class AddOvalCoverageSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns to Tokens table
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

            // Add new columns to TokenAreaCoverages table
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
                name: "SideRadiusKm",
                table: "TokenAreaCoverages",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RotationDegrees",
                table: "TokenAreaCoverages",
                type: "decimal(8,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove columns from TokenAreaCoverages table
            migrationBuilder.DropColumn(
                name: "RotationDegrees",
                table: "TokenAreaCoverages");

            migrationBuilder.DropColumn(
                name: "SideRadiusKm",
                table: "TokenAreaCoverages");

            migrationBuilder.DropColumn(
                name: "RearRadiusKm",
                table: "TokenAreaCoverages");

            migrationBuilder.DropColumn(
                name: "FrontRadiusKm",
                table: "TokenAreaCoverages");

            // Remove columns from Tokens table
            migrationBuilder.DropColumn(
                name: "SideCoverageKm",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "RearCoverageKm",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "FrontCoverageKm",
                table: "Tokens");
        }
    }
}
