using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCursus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_rel_cursus_goal_snapshot_user_cursus_id_choice_group_goal_id",
                table: "rel_cursus_goal_snapshot");

            migrationBuilder.DropColumn(
                name: "choice_group",
                table: "rel_cursus_goal_snapshot");

            migrationBuilder.DropColumn(
                name: "choice_group",
                table: "rel_cursus_goal");

            migrationBuilder.RenameColumn(
                name: "avatar_url",
                table: "tbl_goals",
                newName: "thumbnail_url");

            migrationBuilder.RenameColumn(
                name: "active",
                table: "tbl_goals",
                newName: "enabled");

            migrationBuilder.RenameColumn(
                name: "avatar_url",
                table: "tbl_cursus",
                newName: "thumbnail_url");

            migrationBuilder.RenameColumn(
                name: "active",
                table: "tbl_cursus",
                newName: "enabled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "thumbnail_url",
                table: "tbl_goals",
                newName: "avatar_url");

            migrationBuilder.RenameColumn(
                name: "enabled",
                table: "tbl_goals",
                newName: "active");

            migrationBuilder.RenameColumn(
                name: "thumbnail_url",
                table: "tbl_cursus",
                newName: "avatar_url");

            migrationBuilder.RenameColumn(
                name: "enabled",
                table: "tbl_cursus",
                newName: "active");

            migrationBuilder.AddColumn<Guid>(
                name: "choice_group",
                table: "rel_cursus_goal_snapshot",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "choice_group",
                table: "rel_cursus_goal",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_rel_cursus_goal_snapshot_user_cursus_id_choice_group_goal_id",
                table: "rel_cursus_goal_snapshot",
                columns: new[] { "user_cursus_id", "choice_group", "goal_id" });
        }
    }
}
