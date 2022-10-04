using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VUTCrewBot.DAL.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KeyValueSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyValueSettings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Meets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    MeetupTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeetTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    MeetupDayTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    DoGenerate = table.Column<bool>(type: "INTEGER", nullable: false),
                    RepeatDay = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Responses",
                columns: table => new
                {
                    MeetId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Response = table.Column<byte>(type: "INTEGER", nullable: false, defaultValue: (byte)0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responses", x => new { x.MeetId, x.UserId });
                    table.ForeignKey(
                        name: "FK_Responses_Meets_MeetId",
                        column: x => x.MeetId,
                        principalTable: "Meets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeetTemplateUserEntity",
                columns: table => new
                {
                    MeetTemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetTemplateUserEntity", x => new { x.MeetTemplateId, x.UserId });
                    table.ForeignKey(
                        name: "FK_MeetTemplateUserEntity_MeetTemplates_MeetTemplateId",
                        column: x => x.MeetTemplateId,
                        principalTable: "MeetTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeyValueSettings");

            migrationBuilder.DropTable(
                name: "MeetTemplateUserEntity");

            migrationBuilder.DropTable(
                name: "Responses");

            migrationBuilder.DropTable(
                name: "MeetTemplates");

            migrationBuilder.DropTable(
                name: "Meets");
        }
    }
}
