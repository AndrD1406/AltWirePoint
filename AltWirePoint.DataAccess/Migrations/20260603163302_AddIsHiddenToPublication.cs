using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AltWirePoint.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIsHiddenToPublication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Publications",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Publications");
        }
    }
}
