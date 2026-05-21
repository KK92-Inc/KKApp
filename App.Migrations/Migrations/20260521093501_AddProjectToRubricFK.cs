using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectToRubricFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_user_project_tbl_rubric_rubric_id",
                table: "tbl_user_project");

            migrationBuilder.RenameColumn(
                name: "rubric_id",
                table: "tbl_user_project",
                newName: "RubricId");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_user_project_rubric_id",
                table: "tbl_user_project",
                newName: "IX_tbl_user_project_RubricId");

            migrationBuilder.AddColumn<Guid>(
                name: "project_id",
                table: "tbl_rubric",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rubric_ref",
                table: "tbl_review",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_rubric_project_id",
                table: "tbl_rubric",
                column: "project_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_rubric_tbl_projects_project_id",
                table: "tbl_rubric",
                column: "project_id",
                principalTable: "tbl_projects",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_user_project_tbl_rubric_RubricId",
                table: "tbl_user_project",
                column: "RubricId",
                principalTable: "tbl_rubric",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_rubric_tbl_projects_project_id",
                table: "tbl_rubric");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_user_project_tbl_rubric_RubricId",
                table: "tbl_user_project");

            migrationBuilder.DropIndex(
                name: "IX_tbl_rubric_project_id",
                table: "tbl_rubric");

            migrationBuilder.DropColumn(
                name: "project_id",
                table: "tbl_rubric");

            migrationBuilder.DropColumn(
                name: "rubric_ref",
                table: "tbl_review");

            migrationBuilder.RenameColumn(
                name: "RubricId",
                table: "tbl_user_project",
                newName: "rubric_id");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_user_project_RubricId",
                table: "tbl_user_project",
                newName: "IX_tbl_user_project_rubric_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_user_project_tbl_rubric_rubric_id",
                table: "tbl_user_project",
                column: "rubric_id",
                principalTable: "tbl_rubric",
                principalColumn: "id");
        }
    }
}
