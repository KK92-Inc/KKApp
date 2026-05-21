using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class WorkspaceChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "tbl_rubric",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_rubric_WorkspaceId",
                table: "tbl_rubric",
                column: "WorkspaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_rubric_tbl_workspace_WorkspaceId",
                table: "tbl_rubric",
                column: "WorkspaceId",
                principalTable: "tbl_workspace",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_rubric_tbl_workspace_WorkspaceId",
                table: "tbl_rubric");

            migrationBuilder.DropIndex(
                name: "IX_tbl_rubric_WorkspaceId",
                table: "tbl_rubric");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "tbl_rubric");
        }
    }
}
