using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SyncModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_user_project_tbl_git_git_info_id",
                table: "tbl_user_project");

            migrationBuilder.AlterColumn<Guid>(
                name: "git_info_id",
                table: "tbl_user_project",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_user_project_tbl_git_git_info_id",
                table: "tbl_user_project",
                column: "git_info_id",
                principalTable: "tbl_git",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_user_project_tbl_git_git_info_id",
                table: "tbl_user_project");

            migrationBuilder.AlterColumn<Guid>(
                name: "git_info_id",
                table: "tbl_user_project",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_user_project_tbl_git_git_info_id",
                table: "tbl_user_project",
                column: "git_info_id",
                principalTable: "tbl_git",
                principalColumn: "id");
        }
    }
}
