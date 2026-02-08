using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddCursusGoalRelationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "variant",
                table: "tbl_cursus",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "rel_cursus_goal",
                columns: table => new
                {
                    cursus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    goal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    parent_goal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rel_cursus_goal", x => new { x.cursus_id, x.goal_id });
                    table.ForeignKey(
                        name: "FK_rel_cursus_goal_tbl_cursus_cursus_id",
                        column: x => x.cursus_id,
                        principalTable: "tbl_cursus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rel_cursus_goal_tbl_goals_goal_id",
                        column: x => x.goal_id,
                        principalTable: "tbl_goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rel_cursus_goal_cursus_id_parent_goal_id",
                table: "rel_cursus_goal",
                columns: new[] { "cursus_id", "parent_goal_id" });

            migrationBuilder.CreateIndex(
                name: "IX_rel_cursus_goal_goal_id",
                table: "rel_cursus_goal",
                column: "goal_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rel_cursus_goal");

            migrationBuilder.DropColumn(
                name: "variant",
                table: "tbl_cursus");
        }
    }
}
