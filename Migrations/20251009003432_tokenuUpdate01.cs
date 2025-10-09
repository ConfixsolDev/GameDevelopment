using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class tokenuUpdate01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverageRadiusKm",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "RadiusKm",
                table: "TokenAreaCoverages");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationLevel",
                table: "Tokens",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitDesignation",
                table: "Tokens",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitType",
                table: "Tokens",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizationLevel",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "UnitDesignation",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "UnitType",
                table: "Tokens");

            migrationBuilder.AddColumn<decimal>(
                name: "CoverageRadiusKm",
                table: "Tokens",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RadiusKm",
                table: "TokenAreaCoverages",
                type: "decimal(8,2)",
                nullable: true);
        }
    }
}
