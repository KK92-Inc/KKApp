using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnotationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "markdown",
                table: "tbl_rubric");

            migrationBuilder.RenameColumn(
                name: "rubric_ref",
                table: "tbl_review",
                newName: "ref");

            migrationBuilder.CreateTable(
                name: "tbl_annotation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_annotation", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_annotation_tbl_review_review_id",
                        column: x => x.review_id,
                        principalTable: "tbl_review",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_annotation_tbl_user_author_id",
                        column: x => x.author_id,
                        principalTable: "tbl_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_annotation_author_id",
                table: "tbl_annotation",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_annotation_review_id",
                table: "tbl_annotation",
                column: "review_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_annotation");

            migrationBuilder.RenameColumn(
                name: "ref",
                table: "tbl_review",
                newName: "rubric_ref");

            migrationBuilder.AddColumn<string>(
                name: "markdown",
                table: "tbl_rubric",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
