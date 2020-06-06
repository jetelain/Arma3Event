using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Arma3Event.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Faction",
                columns: table => new
                {
                    FactionID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Flag = table.Column<string>(nullable: true),
                    GameMarker = table.Column<int>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    UsualSide = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faction", x => x.FactionID);
                });

            migrationBuilder.CreateTable(
                name: "Maps",
                columns: table => new
                {
                    GameMapID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Image = table.Column<string>(nullable: true),
                    WorkshopLink = table.Column<string>(nullable: true),
                    WebMap = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maps", x => x.GameMapID);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    SteamId = table.Column<string>(maxLength: 50, nullable: true),
                    SteamName = table.Column<string>(maxLength: 100, nullable: true),
                    PrivacyOptions = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Match",
                columns: table => new
                {
                    MatchID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Image = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    ShortDescription = table.Column<string>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: false),
                    InviteOnly = table.Column<bool>(nullable: false),
                    Template = table.Column<int>(nullable: false),
                    GameMapID = table.Column<int>(nullable: true),
                    DiscordLink = table.Column<string>(nullable: true),
                    RulesLink = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Match", x => x.MatchID);
                    table.ForeignKey(
                        name: "FK_Match_Maps_GameMapID",
                        column: x => x.GameMapID,
                        principalTable: "Maps",
                        principalColumn: "GameMapID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MatchSide",
                columns: table => new
                {
                    MatchSideID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MatchID = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Number = table.Column<int>(nullable: false),
                    MaxUsersCount = table.Column<int>(nullable: false),
                    SquadsPolicy = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchSide", x => x.MatchSideID);
                    table.ForeignKey(
                        name: "FK_MatchSide_Match_MatchID",
                        column: x => x.MatchID,
                        principalTable: "Match",
                        principalColumn: "MatchID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchTechnicalInfos",
                columns: table => new
                {
                    MatchTechnicalInfosID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MatchID = table.Column<int>(nullable: false),
                    GameServerAddress = table.Column<string>(nullable: true),
                    GameServerPort = table.Column<int>(nullable: true),
                    GameServerPassword = table.Column<string>(nullable: true),
                    VoipSystem = table.Column<int>(nullable: false),
                    VoipServerAddress = table.Column<string>(nullable: true),
                    VoipServerPort = table.Column<int>(nullable: true),
                    VoipServerPassword = table.Column<string>(nullable: true),
                    ModsCount = table.Column<int>(nullable: false),
                    ModsDefinition = table.Column<string>(nullable: true),
                    ModsLastChange = table.Column<DateTime>(nullable: true),
                    HoursBeforeRevealPasswords = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchTechnicalInfos", x => x.MatchTechnicalInfosID);
                    table.ForeignKey(
                        name: "FK_MatchTechnicalInfos_Match_MatchID",
                        column: x => x.MatchID,
                        principalTable: "Match",
                        principalColumn: "MatchID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Round",
                columns: table => new
                {
                    RoundID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<int>(nullable: false),
                    MatchID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Round", x => x.RoundID);
                    table.ForeignKey(
                        name: "FK_Round_Match_MatchID",
                        column: x => x.MatchID,
                        principalTable: "Match",
                        principalColumn: "MatchID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchUser",
                columns: table => new
                {
                    MatchUserID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MatchID = table.Column<int>(nullable: false),
                    MatchSideID = table.Column<int>(nullable: true),
                    UserID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchUser", x => x.MatchUserID);
                    table.ForeignKey(
                        name: "FK_MatchUser_Match_MatchID",
                        column: x => x.MatchID,
                        principalTable: "Match",
                        principalColumn: "MatchID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchUser_MatchSide_MatchSideID",
                        column: x => x.MatchSideID,
                        principalTable: "MatchSide",
                        principalColumn: "MatchSideID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchUser_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoundSide",
                columns: table => new
                {
                    RoundSideID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoundID = table.Column<int>(nullable: false),
                    MatchSideID = table.Column<int>(nullable: false),
                    GameSide = table.Column<int>(nullable: false),
                    FactionID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoundSide", x => x.RoundSideID);
                    table.ForeignKey(
                        name: "FK_RoundSide_Faction_FactionID",
                        column: x => x.FactionID,
                        principalTable: "Faction",
                        principalColumn: "FactionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoundSide_MatchSide_MatchSideID",
                        column: x => x.MatchSideID,
                        principalTable: "MatchSide",
                        principalColumn: "MatchSideID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoundSide_Round_RoundID",
                        column: x => x.RoundID,
                        principalTable: "Round",
                        principalColumn: "RoundID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoundSquad",
                columns: table => new
                {
                    RoundSquadID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UniqueDesignation = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    RoundSideID = table.Column<int>(nullable: false),
                    RestrictTeamComposition = table.Column<bool>(nullable: false),
                    InviteOnly = table.Column<bool>(nullable: false),
                    SlotsCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoundSquad", x => x.RoundSquadID);
                    table.ForeignKey(
                        name: "FK_RoundSquad_RoundSide_RoundSideID",
                        column: x => x.RoundSideID,
                        principalTable: "RoundSide",
                        principalColumn: "RoundSideID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MapMarkers",
                columns: table => new
                {
                    MapMarkerID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MatchID = table.Column<int>(nullable: false),
                    RoundSideID = table.Column<int>(nullable: true),
                    RoundSquadID = table.Column<int>(nullable: true),
                    MatchUserID = table.Column<int>(nullable: true),
                    MarkerData = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapMarkers", x => x.MapMarkerID);
                    table.ForeignKey(
                        name: "FK_MapMarkers_Match_MatchID",
                        column: x => x.MatchID,
                        principalTable: "Match",
                        principalColumn: "MatchID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MapMarkers_MatchUser_MatchUserID",
                        column: x => x.MatchUserID,
                        principalTable: "MatchUser",
                        principalColumn: "MatchUserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MapMarkers_RoundSide_RoundSideID",
                        column: x => x.RoundSideID,
                        principalTable: "RoundSide",
                        principalColumn: "RoundSideID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MapMarkers_RoundSquad_RoundSquadID",
                        column: x => x.RoundSquadID,
                        principalTable: "RoundSquad",
                        principalColumn: "RoundSquadID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RoundSlot",
                columns: table => new
                {
                    RoundSlotID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SlotNumber = table.Column<int>(nullable: false),
                    RoundSquadID = table.Column<int>(nullable: false),
                    Label = table.Column<string>(nullable: true),
                    MatchUserID = table.Column<int>(nullable: true),
                    Timestamp = table.Column<long>(nullable: true),
                    Role = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoundSlot", x => x.RoundSlotID);
                    table.ForeignKey(
                        name: "FK_RoundSlot_MatchUser_MatchUserID",
                        column: x => x.MatchUserID,
                        principalTable: "MatchUser",
                        principalColumn: "MatchUserID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RoundSlot_RoundSquad_RoundSquadID",
                        column: x => x.RoundSquadID,
                        principalTable: "RoundSquad",
                        principalColumn: "RoundSquadID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MapMarkers_MatchID",
                table: "MapMarkers",
                column: "MatchID");

            migrationBuilder.CreateIndex(
                name: "IX_MapMarkers_MatchUserID",
                table: "MapMarkers",
                column: "MatchUserID");

            migrationBuilder.CreateIndex(
                name: "IX_MapMarkers_RoundSideID",
                table: "MapMarkers",
                column: "RoundSideID");

            migrationBuilder.CreateIndex(
                name: "IX_MapMarkers_RoundSquadID",
                table: "MapMarkers",
                column: "RoundSquadID");

            migrationBuilder.CreateIndex(
                name: "IX_Match_GameMapID",
                table: "Match",
                column: "GameMapID");

            migrationBuilder.CreateIndex(
                name: "IX_MatchSide_MatchID",
                table: "MatchSide",
                column: "MatchID");

            migrationBuilder.CreateIndex(
                name: "IX_MatchTechnicalInfos_MatchID",
                table: "MatchTechnicalInfos",
                column: "MatchID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchUser_MatchID",
                table: "MatchUser",
                column: "MatchID");

            migrationBuilder.CreateIndex(
                name: "IX_MatchUser_MatchSideID",
                table: "MatchUser",
                column: "MatchSideID");

            migrationBuilder.CreateIndex(
                name: "IX_MatchUser_UserID",
                table: "MatchUser",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Round_MatchID",
                table: "Round",
                column: "MatchID");

            migrationBuilder.CreateIndex(
                name: "IX_RoundSide_FactionID",
                table: "RoundSide",
                column: "FactionID");

            migrationBuilder.CreateIndex(
                name: "IX_RoundSide_MatchSideID",
                table: "RoundSide",
                column: "MatchSideID");

            migrationBuilder.CreateIndex(
                name: "IX_RoundSide_RoundID",
                table: "RoundSide",
                column: "RoundID");

            migrationBuilder.CreateIndex(
                name: "IX_RoundSlot_MatchUserID",
                table: "RoundSlot",
                column: "MatchUserID");

            migrationBuilder.CreateIndex(
                name: "IX_RoundSlot_RoundSquadID",
                table: "RoundSlot",
                column: "RoundSquadID");

            migrationBuilder.CreateIndex(
                name: "IX_RoundSquad_RoundSideID",
                table: "RoundSquad",
                column: "RoundSideID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MapMarkers");

            migrationBuilder.DropTable(
                name: "MatchTechnicalInfos");

            migrationBuilder.DropTable(
                name: "RoundSlot");

            migrationBuilder.DropTable(
                name: "MatchUser");

            migrationBuilder.DropTable(
                name: "RoundSquad");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "RoundSide");

            migrationBuilder.DropTable(
                name: "Faction");

            migrationBuilder.DropTable(
                name: "MatchSide");

            migrationBuilder.DropTable(
                name: "Round");

            migrationBuilder.DropTable(
                name: "Match");

            migrationBuilder.DropTable(
                name: "Maps");
        }
    }
}
