using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class FixDeadlock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_user_tbl_user_details_details_id",
                table: "tbl_user");

            migrationBuilder.DropIndex(
                name: "IX_tbl_user_details_user_id",
                table: "tbl_user_details");

            migrationBuilder.DropIndex(
                name: "IX_tbl_user_details_id",
                table: "tbl_user");

            migrationBuilder.DropColumn(
                name: "details_id",
                table: "tbl_user");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_details_user_id",
                table: "tbl_user_details",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tbl_user_details_user_id",
                table: "tbl_user_details");

            migrationBuilder.AddColumn<Guid>(
                name: "details_id",
                table: "tbl_user",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_details_user_id",
                table: "tbl_user_details",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_details_id",
                table: "tbl_user",
                column: "details_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_user_tbl_user_details_details_id",
                table: "tbl_user",
                column: "details_id",
                principalTable: "tbl_user_details",
                principalColumn: "id");
        }
    }
}
