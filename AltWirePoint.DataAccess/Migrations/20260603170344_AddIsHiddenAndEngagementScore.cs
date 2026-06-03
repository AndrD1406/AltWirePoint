using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AltWirePoint.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIsHiddenAndEngagementScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EngagementScore",
                table: "Publications",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EngagementScore",
                table: "Publications");
        }
    }
}
