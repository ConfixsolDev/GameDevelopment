using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class scenarioplaning01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TokenId",
                table: "UnitDeployments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "EngagementRule",
                table: "MovementOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "MovementOrders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenId",
                table: "UnitDeployments");

            migrationBuilder.DropColumn(
                name: "EngagementRule",
                table: "MovementOrders");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "MovementOrders");
        }
    }
}
