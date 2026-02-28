using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarUrlToProjectGoalCursus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "tbl_projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "tbl_goals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "tbl_cursus",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "tbl_projects");

            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "tbl_goals");

            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "tbl_cursus");
        }
    }
}
