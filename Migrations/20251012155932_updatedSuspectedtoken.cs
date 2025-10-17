using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class updatedSuspectedtoken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MatchingConfidence",
                table: "SuspectedTokens",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PositionAccuracyMeters",
                table: "SuspectedTokens",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RealTokenId",
                table: "SuspectedTokens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuspectedTokens_RealTokenId",
                table: "SuspectedTokens",
                column: "RealTokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_SuspectedTokens_Tokens_RealTokenId",
                table: "SuspectedTokens",
                column: "RealTokenId",
                principalTable: "Tokens",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SuspectedTokens_Tokens_RealTokenId",
                table: "SuspectedTokens");

            migrationBuilder.DropIndex(
                name: "IX_SuspectedTokens_RealTokenId",
                table: "SuspectedTokens");

            migrationBuilder.DropColumn(
                name: "MatchingConfidence",
                table: "SuspectedTokens");

            migrationBuilder.DropColumn(
                name: "PositionAccuracyMeters",
                table: "SuspectedTokens");

            migrationBuilder.DropColumn(
                name: "RealTokenId",
                table: "SuspectedTokens");
        }
    }
}
