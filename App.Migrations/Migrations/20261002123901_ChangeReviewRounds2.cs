using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChangeReviewRounds2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sha",
                table: "tbl_review_round",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sha",
                table: "tbl_review",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sha",
                table: "tbl_review_round");

            migrationBuilder.DropColumn(
                name: "sha",
                table: "tbl_review");
        }
    }
}
