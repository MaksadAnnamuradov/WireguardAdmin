using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WireguardAdmin.Migrations
{
    public partial class ProfileImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfileImageDocumentId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UploadFile",
                columns: table => new
                {
                    DocumentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataFiles = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadFile", x => x.DocumentId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ProfileImageDocumentId",
                table: "AspNetUsers",
                column: "ProfileImageDocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_UploadFile_ProfileImageDocumentId",
                table: "AspNetUsers",
                column: "ProfileImageDocumentId",
                principalTable: "UploadFile",
                principalColumn: "DocumentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UploadFile_ProfileImageDocumentId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "UploadFile");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ProfileImageDocumentId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfileImageDocumentId",
                table: "AspNetUsers");
        }
    }
}
