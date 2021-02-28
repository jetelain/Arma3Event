using Microsoft.EntityFrameworkCore.Migrations;

namespace Arma3Event.Migrations
{
    public partial class TacMap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TacMapId",
                table: "Match",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorldName",
                table: "Match",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TacMapId",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "WorldName",
                table: "Match");
        }
    }
}
