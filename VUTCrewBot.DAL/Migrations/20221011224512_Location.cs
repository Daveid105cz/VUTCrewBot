using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VUTCrewBot.DAL.Migrations
{
    public partial class Location : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "MeetTemplates",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Meets",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "MeetTemplates");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Meets");
        }
    }
}
