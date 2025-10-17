using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCodeForDefence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ManeuverForm",
                table: "AttackIntents",
                newName: "NatoAttackType");

            migrationBuilder.RenameColumn(
                name: "AttackType",
                table: "AttackIntents",
                newName: "CoordinationType");

            migrationBuilder.AddColumn<string>(
                name: "AttackIntensity",
                table: "AttackIntents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AttackPreparation",
                table: "AttackIntents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DefenseElements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ElementId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Coordinates = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Strength = table.Column<int>(type: "int", nullable: false),
                    Effectiveness = table.Column<double>(type: "float", nullable: false),
                    Visibility = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefenseElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DefenseElements_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DefenseElements_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DefenseElements_Tokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefenseElements_Category",
                table: "DefenseElements",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_DefenseElements_ElementId",
                table: "DefenseElements",
                column: "ElementId");

            migrationBuilder.CreateIndex(
                name: "IX_DefenseElements_GameSessionId",
                table: "DefenseElements",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_DefenseElements_Status",
                table: "DefenseElements",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DefenseElements_TeamId",
                table: "DefenseElements",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_DefenseElements_TokenId",
                table: "DefenseElements",
                column: "TokenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefenseElements");

            migrationBuilder.DropColumn(
                name: "AttackIntensity",
                table: "AttackIntents");

            migrationBuilder.DropColumn(
                name: "AttackPreparation",
                table: "AttackIntents");

            migrationBuilder.RenameColumn(
                name: "NatoAttackType",
                table: "AttackIntents",
                newName: "ManeuverForm");

            migrationBuilder.RenameColumn(
                name: "CoordinationType",
                table: "AttackIntents",
                newName: "AttackType");
        }
    }
}
