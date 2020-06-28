using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Arma3Event.Migrations
{
    public partial class Videos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Video",
                columns: table => new
                {
                    VideoID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VideoLink = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Image = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    MatchID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Video", x => x.VideoID);
                    table.ForeignKey(
                        name: "FK_Video_Match_MatchID",
                        column: x => x.MatchID,
                        principalTable: "Match",
                        principalColumn: "MatchID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Video_MatchID",
                table: "Video",
                column: "MatchID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Video");
        }
    }
}
