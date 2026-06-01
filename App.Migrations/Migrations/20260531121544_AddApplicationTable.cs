using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_application",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kc_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    client_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    redirect_uris = table.Column<string[]>(type: "text[]", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_application", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_application_tbl_workspace_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "tbl_workspace",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_application_kc_id",
                table: "tbl_application",
                column: "kc_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_application_workspace_id",
                table: "tbl_application",
                column: "workspace_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_application");
        }
    }
}
