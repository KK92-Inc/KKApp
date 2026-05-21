using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ProperColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_rubric_tbl_workspace_WorkspaceId",
                table: "tbl_rubric");

            migrationBuilder.RenameColumn(
                name: "WorkspaceId",
                table: "tbl_rubric",
                newName: "workspace_id");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_rubric_WorkspaceId",
                table: "tbl_rubric",
                newName: "IX_tbl_rubric_workspace_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "tbl_rubric",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_rubric_tbl_workspace_workspace_id",
                table: "tbl_rubric",
                column: "workspace_id",
                principalTable: "tbl_workspace",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_rubric_tbl_workspace_workspace_id",
                table: "tbl_rubric");

            migrationBuilder.RenameColumn(
                name: "workspace_id",
                table: "tbl_rubric",
                newName: "WorkspaceId");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_rubric_workspace_id",
                table: "tbl_rubric",
                newName: "IX_tbl_rubric_WorkspaceId");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkspaceId",
                table: "tbl_rubric",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_rubric_tbl_workspace_WorkspaceId",
                table: "tbl_rubric",
                column: "WorkspaceId",
                principalTable: "tbl_workspace",
                principalColumn: "id");
        }
    }
}
