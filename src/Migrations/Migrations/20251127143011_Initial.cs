using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cursus",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    track = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cursus", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "goals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goals", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_git",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    ownership = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_git", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    descriptor = table.Column<int>(type: "integer", nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    notifiable_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    GitId = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                    table.ForeignKey(
                        name: "FK_projects_tbl_git_GitId",
                        column: x => x.GitId,
                        principalTable: "tbl_git",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_comment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewId = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_comment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_review",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    RubricId = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_review", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_rubric",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    markdown = table.Column<string>(type: "text", nullable: false),
                    @public = table.Column<bool>(name: "public", type: "boolean", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    git_info_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_rubric", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_rubric_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_rubric_tbl_git_git_info_id",
                        column: x => x.git_info_id,
                        principalTable: "tbl_git",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_user_project",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    git_info_id = table.Column<Guid>(type: "uuid", nullable: true),
                    RubricId = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user_project", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_user_project_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_user_project_tbl_git_git_info_id",
                        column: x => x.git_info_id,
                        principalTable: "tbl_git",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tbl_user_project_tbl_rubric_RubricId",
                        column: x => x.RubricId,
                        principalTable: "tbl_rubric",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    login = table.Column<string>(type: "text", nullable: false),
                    display = table.Column<string>(type: "text", nullable: true),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    details_id = table.Column<Guid>(type: "uuid", nullable: true),
                    UserProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_user_tbl_user_project_UserProjectId",
                        column: x => x.UserProjectId,
                        principalTable: "tbl_user_project",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_user_cursus",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cursus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    track = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user_cursus", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_user_cursus_cursus_cursus_id",
                        column: x => x.cursus_id,
                        principalTable: "cursus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_user_cursus_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_user_details",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    bio = table.Column<string>(type: "character varying(16384)", maxLength: 16384, nullable: true),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    enabled_notifications = table.Column<int>(type: "integer", nullable: false),
                    github_url = table.Column<string>(type: "text", nullable: true),
                    linkedin_url = table.Column<string>(type: "text", nullable: true),
                    reddit_url = table.Column<string>(type: "text", nullable: true),
                    website_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_user_details_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_user_goal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    goal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_cursus_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user_goal", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_user_goal_goals_goal_id",
                        column: x => x.goal_id,
                        principalTable: "goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_user_goal_tbl_user_cursus_user_cursus_id",
                        column: x => x.user_cursus_id,
                        principalTable: "tbl_user_cursus",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tbl_user_goal_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cursus_name",
                table: "cursus",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_cursus_slug",
                table: "cursus",
                column: "slug");

            migrationBuilder.CreateIndex(
                name: "IX_goals_name",
                table: "goals",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_goals_slug",
                table: "goals",
                column: "slug");

            migrationBuilder.CreateIndex(
                name: "IX_projects_GitId",
                table: "projects",
                column: "GitId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_name",
                table: "projects",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_projects_slug",
                table: "projects",
                column: "slug");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_comment_entity_id",
                table: "tbl_comment",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_comment_ReviewId",
                table: "tbl_comment",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_comment_user_id",
                table: "tbl_comment",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_notifications_notifiable_id_read_at",
                table: "tbl_notifications",
                columns: new[] { "notifiable_id", "read_at" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_review_reviewer_id",
                table: "tbl_review",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_review_RubricId",
                table: "tbl_review",
                column: "RubricId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_rubric_creator_id",
                table: "tbl_rubric",
                column: "creator_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_rubric_git_info_id",
                table: "tbl_rubric",
                column: "git_info_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_rubric_name",
                table: "tbl_rubric",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_rubric_project_id",
                table: "tbl_rubric",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_details_id",
                table: "tbl_user",
                column: "details_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_login",
                table: "tbl_user",
                column: "login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_login_display",
                table: "tbl_user",
                columns: new[] { "login", "display" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_UserProjectId",
                table: "tbl_user",
                column: "UserProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_cursus_cursus_id",
                table: "tbl_user_cursus",
                column: "cursus_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_cursus_user_id",
                table: "tbl_user_cursus",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_details_user_id",
                table: "tbl_user_details",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_goal_goal_id",
                table: "tbl_user_goal",
                column: "goal_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_goal_user_cursus_id",
                table: "tbl_user_goal",
                column: "user_cursus_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_goal_user_id",
                table: "tbl_user_goal",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_project_git_info_id",
                table: "tbl_user_project",
                column: "git_info_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_project_project_id",
                table: "tbl_user_project",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_project_RubricId",
                table: "tbl_user_project",
                column: "RubricId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_comment_tbl_review_ReviewId",
                table: "tbl_comment",
                column: "ReviewId",
                principalTable: "tbl_review",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_comment_tbl_user_user_id",
                table: "tbl_comment",
                column: "user_id",
                principalTable: "tbl_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_review_tbl_rubric_RubricId",
                table: "tbl_review",
                column: "RubricId",
                principalTable: "tbl_rubric",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_review_tbl_user_reviewer_id",
                table: "tbl_review",
                column: "reviewer_id",
                principalTable: "tbl_user",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_rubric_tbl_user_creator_id",
                table: "tbl_rubric",
                column: "creator_id",
                principalTable: "tbl_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_user_tbl_user_details_details_id",
                table: "tbl_user",
                column: "details_id",
                principalTable: "tbl_user_details",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_projects_tbl_git_GitId",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_rubric_tbl_git_git_info_id",
                table: "tbl_rubric");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_user_project_tbl_git_git_info_id",
                table: "tbl_user_project");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_rubric_tbl_user_creator_id",
                table: "tbl_rubric");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_user_details_tbl_user_user_id",
                table: "tbl_user_details");

            migrationBuilder.DropTable(
                name: "tbl_comment");

            migrationBuilder.DropTable(
                name: "tbl_notifications");

            migrationBuilder.DropTable(
                name: "tbl_user_goal");

            migrationBuilder.DropTable(
                name: "tbl_review");

            migrationBuilder.DropTable(
                name: "goals");

            migrationBuilder.DropTable(
                name: "tbl_user_cursus");

            migrationBuilder.DropTable(
                name: "cursus");

            migrationBuilder.DropTable(
                name: "tbl_git");

            migrationBuilder.DropTable(
                name: "tbl_user");

            migrationBuilder.DropTable(
                name: "tbl_user_details");

            migrationBuilder.DropTable(
                name: "tbl_user_project");

            migrationBuilder.DropTable(
                name: "tbl_rubric");

            migrationBuilder.DropTable(
                name: "projects");
        }
    }
}
