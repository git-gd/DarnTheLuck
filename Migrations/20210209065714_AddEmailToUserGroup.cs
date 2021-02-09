using Microsoft.EntityFrameworkCore.Migrations;

namespace DarnTheLuck.Migrations
{
    public partial class AddEmailToUserGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GrantEmail",
                table: "UserGroups",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "UserGroups",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GrantEmail",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "UserGroups");
        }
    }
}
