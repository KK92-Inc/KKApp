using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesToGitTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tbl_user_project_members_user_project_id",
                table: "tbl_user_project_members");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_project_members_user_project_id_user_id",
                table: "tbl_user_project_members",
                columns: new[] { "user_project_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_git_name_owner",
                table: "tbl_git",
                columns: new[] { "name", "owner" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tbl_user_project_members_user_project_id_user_id",
                table: "tbl_user_project_members");

            migrationBuilder.DropIndex(
                name: "IX_tbl_git_name_owner",
                table: "tbl_git");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_project_members_user_project_id",
                table: "tbl_user_project_members",
                column: "user_project_id");
        }
    }
}
