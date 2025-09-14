using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WargameBoard.Web.Migrations
{
    /// <inheritdoc />
    public partial class OPOP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Hexes",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Hexes",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Hexes");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Hexes");
        }
    }
}
