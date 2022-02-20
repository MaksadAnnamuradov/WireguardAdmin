using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WireguardAdmin.Migrations
{
    public partial class Session : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "SessionExpiration",
                table: "NewUsers",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "NewUsers",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionExpiration",
                table: "NewUsers");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "NewUsers");
        }
    }
}
