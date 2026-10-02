using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChangeReviewRounds4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "scheduled_at",
                table: "tbl_review",
                newName: "started_at");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                table: "tbl_review",
                newName: "claimed_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "started_at",
                table: "tbl_review",
                newName: "scheduled_at");

            migrationBuilder.RenameColumn(
                name: "claimed_at",
                table: "tbl_review",
                newName: "expires_at");
        }
    }
}
