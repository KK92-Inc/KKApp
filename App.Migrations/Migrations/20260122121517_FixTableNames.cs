using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class FixTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cursus_workspace_workspace_id",
                table: "cursus");

            migrationBuilder.DropForeignKey(
                name: "FK_goals_workspace_workspace_id",
                table: "goals");

            migrationBuilder.DropForeignKey(
                name: "FK_projects_tbl_git_GitId",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_projects_workspace_workspace_id",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_rel_goal_project_goals_GoalId",
                table: "rel_goal_project");

            migrationBuilder.DropForeignKey(
                name: "FK_rel_goal_project_projects_ProjectId",
                table: "rel_goal_project");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_user_cursus_cursus_cursus_id",
                table: "tbl_user_cursus");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_user_goal_goals_goal_id",
                table: "tbl_user_goal");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_user_project_projects_project_id",
                table: "tbl_user_project");

            migrationBuilder.DropPrimaryKey(
                name: "PK_projects",
                table: "projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_goals",
                table: "goals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cursus",
                table: "cursus");

            migrationBuilder.RenameTable(
                name: "projects",
                newName: "tbl_projects");

            migrationBuilder.RenameTable(
                name: "goals",
                newName: "tbl_goals");

            migrationBuilder.RenameTable(
                name: "cursus",
                newName: "tbl_cursus");

            migrationBuilder.RenameIndex(
                name: "IX_projects_workspace_id",
                table: "tbl_projects",
                newName: "IX_tbl_projects_workspace_id");

            migrationBuilder.RenameIndex(
                name: "IX_projects_slug",
                table: "tbl_projects",
                newName: "IX_tbl_projects_slug");

            migrationBuilder.RenameIndex(
                name: "IX_projects_name",
                table: "tbl_projects",
                newName: "IX_tbl_projects_name");

            migrationBuilder.RenameIndex(
                name: "IX_projects_GitId",
                table: "tbl_projects",
                newName: "IX_tbl_projects_GitId");

            migrationBuilder.RenameIndex(
                name: "IX_goals_workspace_id",
                table: "tbl_goals",
                newName: "IX_tbl_goals_workspace_id");

            migrationBuilder.RenameIndex(
                name: "IX_goals_slug",
                table: "tbl_goals",
                newName: "IX_tbl_goals_slug");

            migrationBuilder.RenameIndex(
                name: "IX_goals_name",
                table: "tbl_goals",
                newName: "IX_tbl_goals_name");

            migrationBuilder.RenameIndex(
                name: "IX_cursus_workspace_id",
                table: "tbl_cursus",
                newName: "IX_tbl_cursus_workspace_id");

            migrationBuilder.RenameIndex(
                name: "IX_cursus_slug",
                table: "tbl_cursus",
                newName: "IX_tbl_cursus_slug");

            migrationBuilder.RenameIndex(
                name: "IX_cursus_name",
                table: "tbl_cursus",
                newName: "IX_tbl_cursus_name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_projects",
                table: "tbl_projects",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_goals",
                table: "tbl_goals",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_cursus",
                table: "tbl_cursus",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_rel_goal_project_tbl_goals_GoalId",
                table: "rel_goal_project",
                column: "GoalId",
                principalTable: "tbl_goals",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_rel_goal_project_tbl_projects_ProjectId",
                table: "rel_goal_project",
                column: "ProjectId",
                principalTable: "tbl_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_tbl_projects_tbl_git_GitId",
                table: "tbl_projects",
                column: "GitId",
                principalTable: "tbl_git",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_projects_workspace_workspace_id",
                table: "tbl_projects",
                column: "workspace_id",
                principalTable: "workspace",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_user_cursus_tbl_cursus_cursus_id",
                table: "tbl_user_cursus",
                column: "cursus_id",
                principalTable: "tbl_cursus",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_user_goal_tbl_goals_goal_id",
                table: "tbl_user_goal",
                column: "goal_id",
                principalTable: "tbl_goals",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_user_project_tbl_projects_project_id",
                table: "tbl_user_project",
                column: "project_id",
                principalTable: "tbl_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_rel_goal_project_tbl_goals_GoalId",
                table: "rel_goal_project");

            migrationBuilder.DropForeignKey(
                name: "FK_rel_goal_project_tbl_projects_ProjectId",
                table: "rel_goal_project");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_cursus_workspace_workspace_id",
                table: "tbl_cursus");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_goals_workspace_workspace_id",
                table: "tbl_goals");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_projects_tbl_git_GitId",
                table: "tbl_projects");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_projects_workspace_workspace_id",
                table: "tbl_projects");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_user_cursus_tbl_cursus_cursus_id",
                table: "tbl_user_cursus");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_user_goal_tbl_goals_goal_id",
                table: "tbl_user_goal");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_user_project_tbl_projects_project_id",
                table: "tbl_user_project");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_projects",
                table: "tbl_projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_goals",
                table: "tbl_goals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_cursus",
                table: "tbl_cursus");

            migrationBuilder.RenameTable(
                name: "tbl_projects",
                newName: "projects");

            migrationBuilder.RenameTable(
                name: "tbl_goals",
                newName: "goals");

            migrationBuilder.RenameTable(
                name: "tbl_cursus",
                newName: "cursus");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_projects_workspace_id",
                table: "projects",
                newName: "IX_projects_workspace_id");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_projects_slug",
                table: "projects",
                newName: "IX_projects_slug");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_projects_name",
                table: "projects",
                newName: "IX_projects_name");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_projects_GitId",
                table: "projects",
                newName: "IX_projects_GitId");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_goals_workspace_id",
                table: "goals",
                newName: "IX_goals_workspace_id");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_goals_slug",
                table: "goals",
                newName: "IX_goals_slug");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_goals_name",
                table: "goals",
                newName: "IX_goals_name");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_cursus_workspace_id",
                table: "cursus",
                newName: "IX_cursus_workspace_id");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_cursus_slug",
                table: "cursus",
                newName: "IX_cursus_slug");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_cursus_name",
                table: "cursus",
                newName: "IX_cursus_name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_projects",
                table: "projects",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_goals",
                table: "goals",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cursus",
                table: "cursus",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_cursus_workspace_workspace_id",
                table: "cursus",
                column: "workspace_id",
                principalTable: "workspace",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_goals_workspace_workspace_id",
                table: "goals",
                column: "workspace_id",
                principalTable: "workspace",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_projects_tbl_git_GitId",
                table: "projects",
                column: "GitId",
                principalTable: "tbl_git",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_workspace_workspace_id",
                table: "projects",
                column: "workspace_id",
                principalTable: "workspace",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_rel_goal_project_goals_GoalId",
                table: "rel_goal_project",
                column: "GoalId",
                principalTable: "goals",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_rel_goal_project_projects_ProjectId",
                table: "rel_goal_project",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_user_cursus_cursus_cursus_id",
                table: "tbl_user_cursus",
                column: "cursus_id",
                principalTable: "cursus",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_user_goal_goals_goal_id",
                table: "tbl_user_goal",
                column: "goal_id",
                principalTable: "goals",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_user_project_projects_project_id",
                table: "tbl_user_project",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
