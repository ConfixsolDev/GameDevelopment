using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class Initial3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArmouredRegiments_Brigades_BrigadeId",
                table: "ArmouredRegiments");

            migrationBuilder.DropForeignKey(
                name: "FK_ArtilleryRegiments_Brigades_BrigadeId",
                table: "ArtilleryRegiments");

            migrationBuilder.DropForeignKey(
                name: "FK_InfantryBattalions_Brigades_BrigadeId",
                table: "InfantryBattalions");

            migrationBuilder.AddColumn<Guid>(
                name: "BrigadeId1",
                table: "InfantryBattalions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DroneTypes",
                table: "InfantryBattalions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Drones",
                table: "InfantryBattalions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "BrigadeId1",
                table: "ArtilleryRegiments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DroneTypes",
                table: "ArtilleryRegiments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Drones",
                table: "ArtilleryRegiments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "BrigadeId1",
                table: "ArmouredRegiments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DroneTypes",
                table: "ArmouredRegiments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Drones",
                table: "ArmouredRegiments",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
                        name: "FK_Recon_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Recon_Tokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InfantryBattalions_BrigadeId1",
                table: "InfantryBattalions",
                column: "BrigadeId1");

            migrationBuilder.CreateIndex(
                name: "IX_ArtilleryRegiments_BrigadeId1",
                table: "ArtilleryRegiments",
                column: "BrigadeId1");

            migrationBuilder.CreateIndex(
                name: "IX_ArmouredRegiments_BrigadeId1",
                table: "ArmouredRegiments",
                column: "BrigadeId1");

            migrationBuilder.CreateIndex(
                name: "IX_Intelligence_TeamId",
                table: "Intelligence",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Intelligence_TokenId",
                table: "Intelligence",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_Recon_TeamId",
                table: "Recon",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Recon_TokenId",
                table: "Recon",
                column: "TokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArmouredRegiments_Brigades_BrigadeId",
                table: "ArmouredRegiments",
                column: "BrigadeId",
                principalTable: "Brigades",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ArmouredRegiments_Brigades_BrigadeId1",
                table: "ArmouredRegiments",
                column: "BrigadeId1",
                principalTable: "Brigades",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ArtilleryRegiments_Brigades_BrigadeId",
                table: "ArtilleryRegiments",
                column: "BrigadeId",
                principalTable: "Brigades",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ArtilleryRegiments_Brigades_BrigadeId1",
                table: "ArtilleryRegiments",
                column: "BrigadeId1",
                principalTable: "Brigades",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InfantryBattalions_Brigades_BrigadeId",
                table: "InfantryBattalions",
                column: "BrigadeId",
                principalTable: "Brigades",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InfantryBattalions_Brigades_BrigadeId1",
                table: "InfantryBattalions",
                column: "BrigadeId1",
                principalTable: "Brigades",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArmouredRegiments_Brigades_BrigadeId",
                table: "ArmouredRegiments");

            migrationBuilder.DropForeignKey(
                name: "FK_ArmouredRegiments_Brigades_BrigadeId1",
                table: "ArmouredRegiments");

            migrationBuilder.DropForeignKey(
                name: "FK_ArtilleryRegiments_Brigades_BrigadeId",
                table: "ArtilleryRegiments");

            migrationBuilder.DropForeignKey(
                name: "FK_ArtilleryRegiments_Brigades_BrigadeId1",
                table: "ArtilleryRegiments");

            migrationBuilder.DropForeignKey(
                name: "FK_InfantryBattalions_Brigades_BrigadeId",
                table: "InfantryBattalions");

            migrationBuilder.DropForeignKey(
                name: "FK_InfantryBattalions_Brigades_BrigadeId1",
                table: "InfantryBattalions");

            migrationBuilder.DropTable(
                name: "Intelligence");

            migrationBuilder.DropTable(
                name: "Recon");

            migrationBuilder.DropIndex(
                name: "IX_InfantryBattalions_BrigadeId1",
                table: "InfantryBattalions");

            migrationBuilder.DropIndex(
                name: "IX_ArtilleryRegiments_BrigadeId1",
                table: "ArtilleryRegiments");

            migrationBuilder.DropIndex(
                name: "IX_ArmouredRegiments_BrigadeId1",
                table: "ArmouredRegiments");

            migrationBuilder.DropColumn(
                name: "BrigadeId1",
                table: "InfantryBattalions");

            migrationBuilder.DropColumn(
                name: "DroneTypes",
                table: "InfantryBattalions");

            migrationBuilder.DropColumn(
                name: "Drones",
                table: "InfantryBattalions");

            migrationBuilder.DropColumn(
                name: "BrigadeId1",
                table: "ArtilleryRegiments");

            migrationBuilder.DropColumn(
                name: "DroneTypes",
                table: "ArtilleryRegiments");

            migrationBuilder.DropColumn(
                name: "Drones",
                table: "ArtilleryRegiments");

            migrationBuilder.DropColumn(
                name: "BrigadeId1",
                table: "ArmouredRegiments");

            migrationBuilder.DropColumn(
                name: "DroneTypes",
                table: "ArmouredRegiments");

            migrationBuilder.DropColumn(
                name: "Drones",
                table: "ArmouredRegiments");

            migrationBuilder.AddForeignKey(
                name: "FK_ArmouredRegiments_Brigades_BrigadeId",
                table: "ArmouredRegiments",
                column: "BrigadeId",
                principalTable: "Brigades",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ArtilleryRegiments_Brigades_BrigadeId",
                table: "ArtilleryRegiments",
                column: "BrigadeId",
                principalTable: "Brigades",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InfantryBattalions_Brigades_BrigadeId",
                table: "InfantryBattalions",
                column: "BrigadeId",
                principalTable: "Brigades",
                principalColumn: "Id");
        }
    }
}
