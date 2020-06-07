using Microsoft.EntityFrameworkCore.Migrations;

namespace Arma3Event.Migrations
{
    public partial class MissionBrief : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MissionBriefLink",
                table: "Match",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MissionBriefLink",
                table: "Match");
        }
    }
}
