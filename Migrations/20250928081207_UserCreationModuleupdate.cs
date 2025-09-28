using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechWebSol.Migrations
{
    /// <inheritdoc />
    public partial class UserCreationModuleupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SubTeamCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TeamCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TeamTypeCode",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Designation",
                table: "AspNetUsers",
                newName: "ForceType");

            migrationBuilder.AddColumn<string>(
                name: "ForceType",
                table: "Teams",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForceType",
                table: "Teams");

            migrationBuilder.RenameColumn(
                name: "ForceType",
                table: "AspNetUsers",
                newName: "Designation");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "AspNetUsers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubTeamCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamTypeCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
