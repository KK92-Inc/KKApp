using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChangeKickoffs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rel_user_kickoff");

            migrationBuilder.AddColumn<Guid>(
                name: "kickoff_id",
                table: "tbl_user",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "closes_at",
                table: "tbl_events",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_kickoff_id",
                table: "tbl_user",
                column: "kickoff_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_user_tbl_kickoffs_kickoff_id",
                table: "tbl_user",
                column: "kickoff_id",
                principalTable: "tbl_kickoffs",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_user_tbl_kickoffs_kickoff_id",
                table: "tbl_user");

            migrationBuilder.DropIndex(
                name: "IX_tbl_user_kickoff_id",
                table: "tbl_user");

            migrationBuilder.DropColumn(
                name: "kickoff_id",
                table: "tbl_user");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "closes_at",
                table: "tbl_events",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "rel_user_kickoff",
                columns: table => new
                {
                    kickoff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rel_user_kickoff", x => new { x.user_id, x.kickoff_id });
                    table.ForeignKey(
                        name: "FK_rel_user_kickoff_tbl_kickoffs_kickoff_id",
                        column: x => x.kickoff_id,
                        principalTable: "tbl_kickoffs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rel_user_kickoff_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rel_user_kickoff_kickoff_id",
                table: "rel_user_kickoff",
                column: "kickoff_id");
        }
    }
}
