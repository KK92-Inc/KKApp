using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddChoiceDropPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "position",
                table: "rel_cursus_goal");

            migrationBuilder.AddColumn<int>(
                name: "completion",
                table: "tbl_cursus",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "choice_group",
                table: "rel_cursus_goal",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "completion",
                table: "tbl_cursus");

            migrationBuilder.DropColumn(
                name: "choice_group",
                table: "rel_cursus_goal");

            migrationBuilder.AddColumn<int>(
                name: "position",
                table: "rel_cursus_goal",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
