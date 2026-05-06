using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AltWirePoint.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCloudStoredFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Publications");

            migrationBuilder.RenameColumn(
                name: "PostedAt",
                table: "Publications",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "Image64",
                table: "Publications",
                newName: "Description");

            migrationBuilder.CreateTable(
                name: "CloudStoredFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FileType = table.Column<int>(type: "integer", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    PublicationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudStoredFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CloudStoredFiles_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CloudStoredFiles_PublicationId",
                table: "CloudStoredFiles",
                column: "PublicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CloudStoredFiles");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Publications",
                newName: "Image64");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Publications",
                newName: "PostedAt");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Publications",
                type: "text",
                nullable: true);
        }
    }
}
