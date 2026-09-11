using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EventChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "background_url",
                table: "tbl_events",
                newName: "thumbnail_url");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ends_at",
                table: "tbl_events",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "capacity",
                table: "tbl_events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "closes_at",
                table: "tbl_events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "threshold",
                table: "tbl_events",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "rel_user_event",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rel_user_event", x => new { x.user_id, x.event_id });
                    table.ForeignKey(
                        name: "FK_rel_user_event_tbl_events_event_id",
                        column: x => x.event_id,
                        principalTable: "tbl_events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rel_user_event_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rel_user_event_event_id",
                table: "rel_user_event",
                column: "event_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rel_user_event");

            migrationBuilder.DropColumn(
                name: "capacity",
                table: "tbl_events");

            migrationBuilder.DropColumn(
                name: "closes_at",
                table: "tbl_events");

            migrationBuilder.DropColumn(
                name: "threshold",
                table: "tbl_events");

            migrationBuilder.RenameColumn(
                name: "thumbnail_url",
                table: "tbl_events",
                newName: "background_url");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ends_at",
                table: "tbl_events",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");
        }
    }
}
