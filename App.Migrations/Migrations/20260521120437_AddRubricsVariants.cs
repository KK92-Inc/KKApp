using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddRubricsVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "supported_variants",
                table: "tbl_rubric");

            migrationBuilder.CreateTable(
                name: "tbl_rubric_variant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rubric_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_rubric_variant", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_rubric_variant_tbl_rubric_rubric_id",
                        column: x => x.rubric_id,
                        principalTable: "tbl_rubric",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_rubric_variant_rubric_id",
                table: "tbl_rubric_variant",
                column: "rubric_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_rubric_variant");

            migrationBuilder.AddColumn<int>(
                name: "supported_variants",
                table: "tbl_rubric",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
