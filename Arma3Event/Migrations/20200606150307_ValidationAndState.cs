using Microsoft.EntityFrameworkCore.Migrations;

namespace Arma3Event.Migrations
{
    public partial class ValidationAndState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsValidated",
                table: "RoundSlot",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Match",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsValidated",
                table: "RoundSlot");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Match");
        }
    }
}
