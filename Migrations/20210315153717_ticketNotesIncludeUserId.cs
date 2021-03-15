using Microsoft.EntityFrameworkCore.Migrations;

namespace DarnTheLuck.Migrations
{
    public partial class ticketNotesIncludeUserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "TicketNotes",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketNotes_UserId",
                table: "TicketNotes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketNotes_AspNetUsers_UserId",
                table: "TicketNotes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketNotes_AspNetUsers_UserId",
                table: "TicketNotes");

            migrationBuilder.DropIndex(
                name: "IX_TicketNotes_UserId",
                table: "TicketNotes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TicketNotes");
        }
    }
}
