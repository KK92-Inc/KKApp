using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MoveMaxMembersToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_members",
                table: "tbl_user_project");

            migrationBuilder.AddColumn<int>(
                name: "max_members",
                table: "tbl_projects",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_members",
                table: "tbl_projects");

            migrationBuilder.AddColumn<int>(
                name: "max_members",
                table: "tbl_user_project",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
