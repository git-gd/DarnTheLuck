using Microsoft.EntityFrameworkCore.Migrations;

namespace DarnTheLuck.Migrations
{
    public partial class TicketCompleted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Tickets",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Serial",
                table: "Tickets",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TechId",
                table: "Tickets",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Model",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Serial",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TechId",
                table: "Tickets");
        }
    }
}
