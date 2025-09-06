using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class TokenSystemUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubSectionCode",
                table: "AspNetUsers",
                newName: "TeamCode");

            migrationBuilder.RenameColumn(
                name: "SectionCode",
                table: "AspNetUsers",
                newName: "SubTeamCode");

            migrationBuilder.AddColumn<string>(
                name: "Angles",
                table: "TokenSignatures",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Center",
                table: "TokenSignatures",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Distances",
                table: "TokenSignatures",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Tokens",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserName",
                table: "Tokens",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamId",
                table: "Tokens",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TokenGroupId",
                table: "Tokens",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FreeTokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TouchCount = table.Column<int>(type: "int", nullable: false),
                    System = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedByUserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Distances = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Angles = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Center = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ComplexSignature = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreeTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SessionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedByUserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TokenGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GroupCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedByUserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeamTokenGroupAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TokenGroupId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignedByUserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamTokenGroupAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamTokenGroupAssignments_TokenGroups_TokenGroupId",
                        column: x => x.TokenGroupId,
                        principalTable: "TokenGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TokenBindings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameSessionId = table.Column<int>(type: "int", nullable: false),
                    TokenGroupId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EntityDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    BoundAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnboundAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BoundByUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BoundByUserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenBindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenBindings_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TokenBindings_TokenGroups_TokenGroupId",
                        column: x => x.TokenGroupId,
                        principalTable: "TokenGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_TokenGroupId",
                table: "Tokens",
                column: "TokenGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamTokenGroupAssignments_TokenGroupId",
                table: "TeamTokenGroupAssignments",
                column: "TokenGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenBindings_GameSessionId",
                table: "TokenBindings",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenBindings_TokenGroupId",
                table: "TokenBindings",
                column: "TokenGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tokens_TokenGroups_TokenGroupId",
                table: "Tokens",
                column: "TokenGroupId",
                principalTable: "TokenGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tokens_TokenGroups_TokenGroupId",
                table: "Tokens");

            migrationBuilder.DropTable(
                name: "FreeTokens");

            migrationBuilder.DropTable(
                name: "TeamTokenGroupAssignments");

            migrationBuilder.DropTable(
                name: "TokenBindings");

            migrationBuilder.DropTable(
                name: "GameSessions");

            migrationBuilder.DropTable(
                name: "TokenGroups");

            migrationBuilder.DropIndex(
                name: "IX_Tokens_TokenGroupId",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "Angles",
                table: "TokenSignatures");

            migrationBuilder.DropColumn(
                name: "Center",
                table: "TokenSignatures");

            migrationBuilder.DropColumn(
                name: "Distances",
                table: "TokenSignatures");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "CreatedByUserName",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "TokenGroupId",
                table: "Tokens");

            migrationBuilder.RenameColumn(
                name: "TeamCode",
                table: "AspNetUsers",
                newName: "SubSectionCode");

            migrationBuilder.RenameColumn(
                name: "SubTeamCode",
                table: "AspNetUsers",
                newName: "SectionCode");
        }
    }
}
