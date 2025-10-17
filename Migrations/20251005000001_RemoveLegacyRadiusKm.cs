using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyRadiusKm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove legacy columns from Tokens table
            migrationBuilder.DropColumn(
                name: "CoverageRadiusKm",
                table: "Tokens");

            // Remove legacy columns from TokenAreaCoverages table
            migrationBuilder.DropColumn(
                name: "RadiusKm",
                table: "TokenAreaCoverages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add back legacy columns if needed to rollback
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
