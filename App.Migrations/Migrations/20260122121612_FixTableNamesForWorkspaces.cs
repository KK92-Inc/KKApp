using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class FixTableNamesForWorkspaces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_cursus_workspace_workspace_id",
                table: "tbl_cursus");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_goals_workspace_workspace_id",
                table: "tbl_goals");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_projects_workspace_workspace_id",
                table: "tbl_projects");

            migrationBuilder.DropForeignKey(
                name: "FK_workspace_tbl_user_owner_id",
                table: "workspace");

            migrationBuilder.DropPrimaryKey(
                name: "PK_workspace",
                table: "workspace");

            migrationBuilder.RenameTable(
                name: "workspace",
                newName: "tbl_workspace");

            migrationBuilder.RenameIndex(
                name: "IX_workspace_owner_id",
                table: "tbl_workspace",
                newName: "IX_tbl_workspace_owner_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_workspace",
                table: "tbl_workspace",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_cursus_tbl_workspace_workspace_id",
                table: "tbl_cursus",
                column: "workspace_id",
                principalTable: "tbl_workspace",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_goals_tbl_workspace_workspace_id",
                table: "tbl_goals",
                column: "workspace_id",
                principalTable: "tbl_workspace",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_projects_tbl_workspace_workspace_id",
                table: "tbl_projects",
                column: "workspace_id",
                principalTable: "tbl_workspace",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_workspace_tbl_user_owner_id",
                table: "tbl_workspace",
                column: "owner_id",
                principalTable: "tbl_user",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_cursus_tbl_workspace_workspace_id",
                table: "tbl_cursus");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_goals_tbl_workspace_workspace_id",
                table: "tbl_goals");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_projects_tbl_workspace_workspace_id",
                table: "tbl_projects");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_workspace_tbl_user_owner_id",
                table: "tbl_workspace");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_workspace",
                table: "tbl_workspace");

            migrationBuilder.RenameTable(
                name: "tbl_workspace",
                newName: "workspace");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_workspace_owner_id",
                table: "workspace",
                newName: "IX_workspace_owner_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_workspace",
                table: "workspace",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_cursus_workspace_workspace_id",
                table: "tbl_cursus",
                column: "workspace_id",
                principalTable: "workspace",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_goals_workspace_workspace_id",
                table: "tbl_goals",
                column: "workspace_id",
                principalTable: "workspace",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_projects_workspace_workspace_id",
                table: "tbl_projects",
                column: "workspace_id",
                principalTable: "workspace",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_workspace_tbl_user_owner_id",
                table: "workspace",
                column: "owner_id",
                principalTable: "tbl_user",
                principalColumn: "id");
        }
    }
}
