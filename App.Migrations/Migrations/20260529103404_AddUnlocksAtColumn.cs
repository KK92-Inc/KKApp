using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddUnlocksAtColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "unlocks_at",
                table: "tbl_user_project",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "unlocks_at",
                table: "tbl_user_goal",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "unlocks_at",
                table: "tbl_user_cursus",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "unlocks_at",
                table: "tbl_user_project");

            migrationBuilder.DropColumn(
                name: "unlocks_at",
                table: "tbl_user_goal");

            migrationBuilder.DropColumn(
                name: "unlocks_at",
                table: "tbl_user_cursus");
        }
    }
}
