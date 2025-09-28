using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class newmapchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentLatitude",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "CurrentLongitude",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MapMarkers");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "MapMarkers");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "MapMarkers");

            migrationBuilder.DropColumn(
                name: "TokenName",
                table: "MapMarkers");

            migrationBuilder.RenameColumn(
                name: "Location",
                table: "MapMarkers",
                newName: "longitude");

            migrationBuilder.AddColumn<string>(
                name: "latitude",
                table: "MapMarkers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "latitude",
                table: "MapMarkers");

            migrationBuilder.RenameColumn(
                name: "longitude",
                table: "MapMarkers",
                newName: "Location");

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

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MapMarkers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "MapMarkers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "MapMarkers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenName",
                table: "MapMarkers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
