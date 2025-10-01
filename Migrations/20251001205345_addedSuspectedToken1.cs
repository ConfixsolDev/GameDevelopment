using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class addedSuspectedToken1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DetectionProbability",
                table: "UnitDeployments",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttackOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttackerTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AxisId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ArtilleryAttached = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MpReservePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Posture = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExpectedStartTurn = table.Column<int>(type: "int", nullable: false),
                    DurationTurns = table.Column<int>(type: "int", nullable: false),
                    ExecutionMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExecutedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PayloadJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DetectionConfidence = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsLowConfidence = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttackOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttackOrders_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AttackOrders_Tokens_AttackerTokenId",
                        column: x => x.AttackerTokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttackOrders_Tokens_TargetTokenId",
                        column: x => x.TargetTokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttackOrders_AttackerTokenId",
                table: "AttackOrders",
                column: "AttackerTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_AttackOrders_ExpectedStartTurn",
                table: "AttackOrders",
                column: "ExpectedStartTurn");

            migrationBuilder.CreateIndex(
                name: "IX_AttackOrders_Status",
                table: "AttackOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AttackOrders_TargetTokenId",
                table: "AttackOrders",
                column: "TargetTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_AttackOrders_TeamId",
                table: "AttackOrders",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttackOrders");

            migrationBuilder.DropColumn(
                name: "DetectionProbability",
                table: "UnitDeployments");
        }
    }
}
