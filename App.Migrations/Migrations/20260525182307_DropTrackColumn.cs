using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class DropTrackColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "track",
                table: "tbl_user_cursus");

            migrationBuilder.DropColumn(
                name: "track",
                table: "tbl_cursus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "track",
                table: "tbl_user_cursus",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "track",
                table: "tbl_cursus",
                type: "jsonb",
                nullable: true);
        }
    }
}
