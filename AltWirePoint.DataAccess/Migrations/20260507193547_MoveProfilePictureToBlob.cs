using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AltWirePoint.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MoveProfilePictureToBlob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Logo",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "CloudStoredFiles",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "PublicationId",
                table: "CloudStoredFiles",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId",
                table: "CloudStoredFiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CloudStoredFiles_ApplicationUserId",
                table: "CloudStoredFiles",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CloudStoredFiles_AspNetUsers_ApplicationUserId",
                table: "CloudStoredFiles",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CloudStoredFiles_AspNetUsers_ApplicationUserId",
                table: "CloudStoredFiles");

            migrationBuilder.DropIndex(
                name: "IX_CloudStoredFiles_ApplicationUserId",
                table: "CloudStoredFiles");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "CloudStoredFiles");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "CloudStoredFiles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048);

            migrationBuilder.AlterColumn<Guid>(
                name: "PublicationId",
                table: "CloudStoredFiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("9c8b8e2e-4f3a-4d2e-bf4a-e5c8a1b2c3d4"),
                column: "Logo",
                value: null);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
                column: "Logo",
                value: null);
        }
    }
}
