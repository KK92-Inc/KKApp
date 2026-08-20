using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class Changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_rubric_tbl_user_creator_id",
                table: "tbl_rubric");

            migrationBuilder.DropIndex(
                name: "IX_tbl_rubric_creator_id",
                table: "tbl_rubric");

            migrationBuilder.DropColumn(
                name: "creator_id",
                table: "tbl_rubric");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "creator_id",
                table: "tbl_rubric",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_tbl_rubric_creator_id",
                table: "tbl_rubric",
                column: "creator_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_rubric_tbl_user_creator_id",
                table: "tbl_rubric",
                column: "creator_id",
                principalTable: "tbl_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
