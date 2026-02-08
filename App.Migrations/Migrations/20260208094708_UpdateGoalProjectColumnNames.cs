using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGoalProjectColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_rel_goal_project_tbl_goals_GoalId",
                table: "rel_goal_project");

            migrationBuilder.DropForeignKey(
                name: "FK_rel_goal_project_tbl_projects_ProjectId",
                table: "rel_goal_project");

            migrationBuilder.RenameColumn(
                name: "GoalId",
                table: "rel_goal_project",
                newName: "goal_id");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "rel_goal_project",
                newName: "project_id");

            migrationBuilder.RenameIndex(
                name: "IX_rel_goal_project_GoalId",
                table: "rel_goal_project",
                newName: "IX_rel_goal_project_goal_id");

            migrationBuilder.AddForeignKey(
                name: "FK_rel_goal_project_tbl_goals_goal_id",
                table: "rel_goal_project",
                column: "goal_id",
                principalTable: "tbl_goals",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_rel_goal_project_tbl_projects_project_id",
                table: "rel_goal_project",
                column: "project_id",
                principalTable: "tbl_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_rel_goal_project_tbl_goals_goal_id",
                table: "rel_goal_project");

            migrationBuilder.DropForeignKey(
                name: "FK_rel_goal_project_tbl_projects_project_id",
                table: "rel_goal_project");

            migrationBuilder.RenameColumn(
                name: "goal_id",
                table: "rel_goal_project",
                newName: "GoalId");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "rel_goal_project",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_rel_goal_project_goal_id",
                table: "rel_goal_project",
                newName: "IX_rel_goal_project_GoalId");

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
        }
    }
}
