using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddSSHGitKeyStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_user_tbl_user_project_UserProjectId",
                table: "tbl_user");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_user_goal_tbl_user_cursus_user_cursus_id",
                table: "tbl_user_goal");

            migrationBuilder.DropIndex(
                name: "IX_tbl_user_goal_user_cursus_id",
                table: "tbl_user_goal");

            migrationBuilder.DropIndex(
                name: "IX_tbl_user_UserProjectId",
                table: "tbl_user");

            migrationBuilder.DropColumn(
                name: "user_cursus_id",
                table: "tbl_user_goal");

            migrationBuilder.DropColumn(
                name: "UserProjectId",
                table: "tbl_user");

            migrationBuilder.AddColumn<int>(
                name: "state",
                table: "tbl_user_goal",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "deprecated",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "public",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "rel_goal_project",
                columns: table => new
                {
                    GoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rel_goal_project", x => new { x.ProjectId, x.GoalId });
                    table.ForeignKey(
                        name: "FK_rel_goal_project_goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rel_goal_project_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ssh_key",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    public_key = table.Column<string>(type: "text", nullable: false),
                    fingerprint = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    key_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    synced_to_git_server = table.Column<bool>(type: "boolean", nullable: false),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ssh_key", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_ssh_key_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_user_project_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    left_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user_project_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_user_project_members_tbl_user_project_user_project_id",
                        column: x => x.user_project_id,
                        principalTable: "tbl_user_project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_user_project_members_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_user_project_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user_project_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_user_project_transactions_tbl_user_project_user_project~",
                        column: x => x.user_project_id,
                        principalTable: "tbl_user_project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_user_project_transactions_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_rel_goal_project_GoalId",
                table: "rel_goal_project",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ssh_key_user_id",
                table: "tbl_ssh_key",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_project_members_user_id",
                table: "tbl_user_project_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_project_members_user_project_id",
                table: "tbl_user_project_members",
                column: "user_project_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_project_transactions_user_id",
                table: "tbl_user_project_transactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_project_transactions_user_project_id",
                table: "tbl_user_project_transactions",
                column: "user_project_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rel_goal_project");

            migrationBuilder.DropTable(
                name: "tbl_ssh_key");

            migrationBuilder.DropTable(
                name: "tbl_user_project_members");

            migrationBuilder.DropTable(
                name: "tbl_user_project_transactions");

            migrationBuilder.DropColumn(
                name: "state",
                table: "tbl_user_goal");

            migrationBuilder.DropColumn(
                name: "active",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "deprecated",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "public",
                table: "projects");

            migrationBuilder.AddColumn<Guid>(
                name: "user_cursus_id",
                table: "tbl_user_goal",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserProjectId",
                table: "tbl_user",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_goal_user_cursus_id",
                table: "tbl_user_goal",
                column: "user_cursus_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_UserProjectId",
                table: "tbl_user",
                column: "UserProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_user_tbl_user_project_UserProjectId",
                table: "tbl_user",
                column: "UserProjectId",
                principalTable: "tbl_user_project",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_user_goal_tbl_user_cursus_user_cursus_id",
                table: "tbl_user_goal",
                column: "user_cursus_id",
                principalTable: "tbl_user_cursus",
                principalColumn: "id");
        }
    }
}
