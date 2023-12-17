using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataPacketLibrary.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Recurrence = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "BlockedApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EventId = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayVersion = table.Column<string>(type: "TEXT", nullable: false),
                    Publisher = table.Column<string>(type: "TEXT", nullable: false),
                    InstallDate = table.Column<string>(type: "TEXT", nullable: false),
                    InstallLocation = table.Column<string>(type: "TEXT", nullable: false),
                    Comments = table.Column<string>(type: "TEXT", nullable: false),
                    UninstallString = table.Column<string>(type: "TEXT", nullable: false),
                    RegistryPath = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockedApplications_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlockedWebsites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EventId = table.Column<int>(type: "INTEGER", nullable: false),
                    BlockAllSites = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedWebsites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockedWebsites_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlockedWebsiteUrls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BlockedWebsiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedWebsiteUrls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockedWebsiteUrls_BlockedWebsites_BlockedWebsiteId",
                        column: x => x.BlockedWebsiteId,
                        principalTable: "BlockedWebsites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlockedApplications_EventId",
                table: "BlockedApplications",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockedWebsites_EventId",
                table: "BlockedWebsites",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlockedWebsiteUrls_BlockedWebsiteId",
                table: "BlockedWebsiteUrls",
                column: "BlockedWebsiteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockedApplications");

            migrationBuilder.DropTable(
                name: "BlockedWebsiteUrls");

            migrationBuilder.DropTable(
                name: "BlockedWebsites");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
