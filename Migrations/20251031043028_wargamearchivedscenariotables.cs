using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class wargamearchivedscenariotables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WarGameArchives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GameCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GameMonth = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GameYear = table.Column<int>(type: "int", nullable: false),
                    CurrentTurn = table.Column<int>(type: "int", nullable: false),
                    TotalTurns = table.Column<int>(type: "int", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MapConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GameStateJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MapOverlaysJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ForcesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttacksJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManeuversJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefenseElementsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GameTurnsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdjudicationResultsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CombatResultsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SavedByUserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SavedByUserName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SavedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PdfDocumentPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarGameArchives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarGameArchives_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WarGameArchives_GameSessionId",
                table: "WarGameArchives",
                column: "GameSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarGameArchives");
        }
    }
}
