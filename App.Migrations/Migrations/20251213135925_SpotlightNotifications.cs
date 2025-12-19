using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SpotlightNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_spotlights",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    action_text = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    href = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    background_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    starts_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_spotlights", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_spotlight_dismissals",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    spotlight_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dismissed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_spotlight_dismissals", x => new { x.user_id, x.spotlight_id });
                    table.ForeignKey(
                        name: "FK_tbl_spotlight_dismissals_tbl_spotlights_spotlight_id",
                        column: x => x.spotlight_id,
                        principalTable: "tbl_spotlights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_spotlight_dismissals_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_spotlight_dismissals_spotlight_id",
                table: "tbl_spotlight_dismissals",
                column: "spotlight_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_spotlights_starts_at_ends_at",
                table: "tbl_spotlights",
                columns: new[] { "starts_at", "ends_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_spotlight_dismissals");

            migrationBuilder.DropTable(
                name: "tbl_spotlights");
        }
    }
}
