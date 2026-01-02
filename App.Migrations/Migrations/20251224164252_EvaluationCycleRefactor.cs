using System;
using System.Collections.Generic;
using App.Backend.Domain.Entities.Reviews;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EvaluationCycleRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_review_tbl_rubric_RubricId",
                table: "tbl_review");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_rubric_projects_project_id",
                table: "tbl_rubric");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_rubric_tbl_git_git_info_id",
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
                name: "namespace",
                table: "tbl_git");

            migrationBuilder.RenameColumn(
                name: "RubricId",
                table: "tbl_user_project",
                newName: "rubric_id");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_user_project_RubricId",
                table: "tbl_user_project",
                newName: "IX_tbl_user_project_rubric_id");

            migrationBuilder.RenameColumn(
                name: "RubricId",
                table: "tbl_review",
                newName: "rubric_id");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_review_RubricId",
                table: "tbl_review",
                newName: "IX_tbl_review_rubric_id");

            migrationBuilder.RenameColumn(
                name: "url",
                table: "tbl_git",
                newName: "owner");

            migrationBuilder.AlterColumn<Guid>(
                name: "git_info_id",
                table: "tbl_rubric",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<List<Rule>>(
                name: "reviewee_eligibility_rules",
                table: "tbl_rubric",
                type: "jsonb",
                nullable: false);

            migrationBuilder.AddColumn<List<Rule>>(
                name: "reviewer_eligibility_rules",
                table: "tbl_rubric",
                type: "jsonb",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "slug",
                table: "tbl_rubric",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "supported_review_kinds",
                table: "tbl_rubric",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "rubric_id",
                table: "tbl_review",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_rubric_slug",
                table: "tbl_rubric",
                column: "slug");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_review_user_project_id",
                table: "tbl_review",
                column: "user_project_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_review_tbl_rubric_rubric_id",
                table: "tbl_review",
                column: "rubric_id",
                principalTable: "tbl_rubric",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_review_tbl_user_project_user_project_id",
                table: "tbl_review",
                column: "user_project_id",
                principalTable: "tbl_user_project",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_rubric_tbl_git_git_info_id",
                table: "tbl_rubric",
                column: "git_info_id",
                principalTable: "tbl_git",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_user_project_tbl_rubric_rubric_id",
                table: "tbl_user_project",
                column: "rubric_id",
                principalTable: "tbl_rubric",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_review_tbl_rubric_rubric_id",
                table: "tbl_review");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_review_tbl_user_project_user_project_id",
                table: "tbl_review");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_rubric_tbl_git_git_info_id",
                table: "tbl_rubric");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_user_project_tbl_rubric_rubric_id",
                table: "tbl_user_project");

            migrationBuilder.DropIndex(
                name: "IX_tbl_rubric_slug",
                table: "tbl_rubric");

            migrationBuilder.DropIndex(
                name: "IX_tbl_review_user_project_id",
                table: "tbl_review");

            migrationBuilder.DropColumn(
                name: "reviewee_eligibility_rules",
                table: "tbl_rubric");

            migrationBuilder.DropColumn(
                name: "reviewer_eligibility_rules",
                table: "tbl_rubric");

            migrationBuilder.DropColumn(
                name: "slug",
                table: "tbl_rubric");

            migrationBuilder.DropColumn(
                name: "supported_review_kinds",
                table: "tbl_rubric");

            migrationBuilder.RenameColumn(
                name: "rubric_id",
                table: "tbl_user_project",
                newName: "RubricId");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_user_project_rubric_id",
                table: "tbl_user_project",
                newName: "IX_tbl_user_project_RubricId");

            migrationBuilder.RenameColumn(
                name: "rubric_id",
                table: "tbl_review",
                newName: "RubricId");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_review_rubric_id",
                table: "tbl_review",
                newName: "IX_tbl_review_RubricId");

            migrationBuilder.RenameColumn(
                name: "owner",
                table: "tbl_git",
                newName: "url");

            migrationBuilder.AlterColumn<Guid>(
                name: "git_info_id",
                table: "tbl_rubric",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "project_id",
                table: "tbl_rubric",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "RubricId",
                table: "tbl_review",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "namespace",
                table: "tbl_git",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_rubric_project_id",
                table: "tbl_rubric",
                column: "project_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_review_tbl_rubric_RubricId",
                table: "tbl_review",
                column: "RubricId",
                principalTable: "tbl_rubric",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_rubric_projects_project_id",
                table: "tbl_rubric",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_rubric_tbl_git_git_info_id",
                table: "tbl_rubric",
                column: "git_info_id",
                principalTable: "tbl_git",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_user_project_tbl_rubric_RubricId",
                table: "tbl_user_project",
                column: "RubricId",
                principalTable: "tbl_rubric",
                principalColumn: "id");
        }
    }
}
