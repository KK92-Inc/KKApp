using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MigrateToMembershipTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_user_project_members");

            migrationBuilder.CreateTable(
                name: "tbl_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<int>(type: "integer", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    git_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    left_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_members_tbl_git_git_id",
                        column: x => x.git_id,
                        principalTable: "tbl_git",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tbl_members_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_members_git_id_user_id",
                table: "tbl_members",
                columns: new[] { "git_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_members_user_id",
                table: "tbl_members",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_members");

            migrationBuilder.CreateTable(
                name: "tbl_user_project_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    left_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    role = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user_project_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_user_project_members_tbl_user_project_user_project_id",
                        column: x => x.user_project_id,
                        principalTable: "tbl_user_project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_user_project_members_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_project_members_user_id",
                table: "tbl_user_project_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_project_members_user_project_id_user_id",
                table: "tbl_user_project_members",
                columns: new[] { "user_project_id", "user_id" },
                unique: true);
        }
    }
}
