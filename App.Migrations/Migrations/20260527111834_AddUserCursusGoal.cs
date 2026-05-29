using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCursusGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rel_cursus_goal_snapshot",
                columns: table => new
                {
                    user_cursus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    goal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_goal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    choice_group = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rel_cursus_goal_snapshot", x => new { x.user_cursus_id, x.goal_id });
                    table.ForeignKey(
                        name: "FK_rel_cursus_goal_snapshot_tbl_goals_goal_id",
                        column: x => x.goal_id,
                        principalTable: "tbl_goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rel_cursus_goal_snapshot_tbl_user_cursus_user_cursus_id",
                        column: x => x.user_cursus_id,
                        principalTable: "tbl_user_cursus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rel_cursus_goal_snapshot_goal_id",
                table: "rel_cursus_goal_snapshot",
                column: "goal_id");

            migrationBuilder.CreateIndex(
                name: "IX_rel_cursus_goal_snapshot_user_cursus_id_choice_group_goal_id",
                table: "rel_cursus_goal_snapshot",
                columns: new[] { "user_cursus_id", "choice_group", "goal_id" });

            migrationBuilder.CreateIndex(
                name: "IX_rel_cursus_goal_snapshot_user_cursus_id_parent_goal_id",
                table: "rel_cursus_goal_snapshot",
                columns: new[] { "user_cursus_id", "parent_goal_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rel_cursus_goal_snapshot");
        }
    }
}
