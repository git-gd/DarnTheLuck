using Microsoft.EntityFrameworkCore.Migrations;

namespace DarnTheLuck.Migrations
{
    public partial class ChangeTechTicketFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TechId",
                table: "Tickets");

            migrationBuilder.AddColumn<string>(
                name: "TechEmail",
                table: "Tickets",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechName",
                table: "Tickets",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TechEmail",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TechName",
                table: "Tickets");

            migrationBuilder.AddColumn<int>(
                name: "TechId",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
