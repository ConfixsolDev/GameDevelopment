using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class updateCodeandData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Tokens");

            migrationBuilder.AddColumn<string>(
                name: "AssetImagePath",
                table: "Tokens",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CoverageRadiusKm",
                table: "Tokens",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentLatitude",
                table: "Tokens",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentLongitude",
                table: "Tokens",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TokenId",
                table: "InfantryBattalions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TokenId",
                table: "ArtilleryRegiments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TokenId",
                table: "ArmouredRegiments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TokenAreaCoverages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Geometry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AreaKm2 = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    RadiusKm = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_TokenAreaCoverages_TokenId",
                table: "TokenAreaCoverages",
                column: "TokenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokenAreaCoverages");

            migrationBuilder.DropColumn(
                name: "AssetImagePath",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "CoverageRadiusKm",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "CurrentLatitude",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "CurrentLongitude",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "TokenId",
                table: "InfantryBattalions");

            migrationBuilder.DropColumn(
                name: "TokenId",
                table: "ArtilleryRegiments");

            migrationBuilder.DropColumn(
                name: "TokenId",
                table: "ArmouredRegiments");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Tokens",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Tokens",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
