using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewRounds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "file_path",
                table: "tbl_annotation");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "finished_at",
                table: "tbl_review",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "passed",
                table: "tbl_review",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "round_id",
                table: "tbl_review",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tbl_review_round",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt = table.Column<int>(type: "integer", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    rubric_id = table.Column<Guid>(type: "uuid", nullable: false),
                    @ref = table.Column<string>(name: "ref", type: "text", nullable: false),
                    requested_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_review_round", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_review_round_tbl_rubric_rubric_id",
                        column: x => x.rubric_id,
                        principalTable: "tbl_rubric",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_review_round_tbl_user_project_user_project_id",
                        column: x => x.user_project_id,
                        principalTable: "tbl_user_project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_review_round_id",
                table: "tbl_review",
                column: "round_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_review_round_rubric_id",
                table: "tbl_review_round",
                column: "rubric_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_review_round_user_project_id_attempt",
                table: "tbl_review_round",
                columns: new[] { "user_project_id", "attempt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_review_round_user_project_id_open",
                table: "tbl_review_round",
                column: "user_project_id",
                unique: true,
                filter: "\"state\" = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_review_tbl_review_round_round_id",
                table: "tbl_review",
                column: "round_id",
                principalTable: "tbl_review_round",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_review_tbl_review_round_round_id",
                table: "tbl_review");

            migrationBuilder.DropTable(
                name: "tbl_review_round");

            migrationBuilder.DropIndex(
                name: "IX_tbl_review_round_id",
                table: "tbl_review");

            migrationBuilder.DropColumn(
                name: "finished_at",
                table: "tbl_review");

            migrationBuilder.DropColumn(
                name: "passed",
                table: "tbl_review");

            migrationBuilder.DropColumn(
                name: "round_id",
                table: "tbl_review");

            migrationBuilder.AddColumn<string>(
                name: "file_path",
                table: "tbl_annotation",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
