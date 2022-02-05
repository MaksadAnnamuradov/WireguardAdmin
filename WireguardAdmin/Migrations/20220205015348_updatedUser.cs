using Microsoft.EntityFrameworkCore.Migrations;

namespace WireguardAdmin.Migrations
{
    public partial class updatedUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientConfigFile",
                table: "Users",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientConfigFile",
                table: "Users");
        }
    }
}
