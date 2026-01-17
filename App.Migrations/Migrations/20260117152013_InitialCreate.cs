using System;
using System.Collections.Generic;
using App.Backend.Domain.Rules;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    owner = table.Column<string>(type: "text", nullable: false),
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
                name: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    @public = table.Column<bool>(name: "public", type: "boolean", nullable: false),
                    deprecated = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "rel_goal_project",
                columns: table => new
                {
                    GoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rel_goal_project", x => new { x.ProjectId, x.GoalId });
                    table.ForeignKey(
                        name: "FK_rel_goal_project_goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rel_goal_project_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    rubric_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    slug = table.Column<string>(type: "text", nullable: false),
                    markdown = table.Column<string>(type: "text", nullable: false),
                    @public = table.Column<bool>(name: "public", type: "boolean", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    supported_variants = table.Column<int>(type: "integer", nullable: false),
                    reviewer_rules = table.Column<ICollection<Rule>>(type: "jsonb", nullable: false),
                    reviewee_rules = table.Column<ICollection<Rule>>(type: "jsonb", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    git_info_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_rubric", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_rubric_tbl_git_git_info_id",
                        column: x => x.git_info_id,
                        principalTable: "tbl_git",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_user_project",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    git_info_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rubric_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                        name: "FK_tbl_user_project_tbl_rubric_rubric_id",
                        column: x => x.rubric_id,
                        principalTable: "tbl_rubric",
                        principalColumn: "id");
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
                });

            migrationBuilder.CreateTable(
                name: "tbl_ssh_key",
                columns: table => new
                {
                    fingerprint = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    blob = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ssh_key", x => x.fingerprint);
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
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user", x => x.id);
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
                    state = table.Column<int>(type: "integer", nullable: false),
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
                        name: "FK_tbl_user_goal_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_user_project_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    left_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "tbl_user_project_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user_project_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_user_project_transactions_tbl_user_project_user_project~",
                        column: x => x.user_project_id,
                        principalTable: "tbl_user_project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_user_project_transactions_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "id");
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
                name: "IX_rel_goal_project_GoalId",
                table: "rel_goal_project",
                column: "GoalId");

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
                name: "IX_tbl_review_rubric_id",
                table: "tbl_review",
                column: "rubric_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_review_user_project_id",
                table: "tbl_review",
                column: "user_project_id");

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
                name: "IX_tbl_rubric_slug",
                table: "tbl_rubric",
                column: "slug");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_spotlight_dismissals_spotlight_id",
                table: "tbl_spotlight_dismissals",
                column: "spotlight_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_spotlights_starts_at_ends_at",
                table: "tbl_spotlights",
                columns: new[] { "starts_at", "ends_at" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ssh_key_user_id",
                table: "tbl_ssh_key",
                column: "user_id");

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
                name: "IX_tbl_user_project_rubric_id",
                table: "tbl_user_project",
                column: "rubric_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_project_members_user_id",
                table: "tbl_user_project_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_project_members_user_project_id",
                table: "tbl_user_project_members",
                column: "user_project_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_project_transactions_user_id",
                table: "tbl_user_project_transactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_project_transactions_user_project_id",
                table: "tbl_user_project_transactions",
                column: "user_project_id");

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
                name: "FK_tbl_review_tbl_rubric_rubric_id",
                table: "tbl_review",
                column: "rubric_id",
                principalTable: "tbl_rubric",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_review_tbl_user_project_user_project_id",
                table: "tbl_review",
                column: "user_project_id",
                principalTable: "tbl_user_project",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_tbl_spotlight_dismissals_tbl_user_user_id",
                table: "tbl_spotlight_dismissals",
                column: "user_id",
                principalTable: "tbl_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_ssh_key_tbl_user_user_id",
                table: "tbl_ssh_key",
                column: "user_id",
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
                name: "FK_tbl_user_details_tbl_user_user_id",
                table: "tbl_user_details");

            migrationBuilder.DropTable(
                name: "rel_goal_project");

            migrationBuilder.DropTable(
                name: "tbl_comment");

            migrationBuilder.DropTable(
                name: "tbl_notifications");

            migrationBuilder.DropTable(
                name: "tbl_spotlight_dismissals");

            migrationBuilder.DropTable(
                name: "tbl_ssh_key");

            migrationBuilder.DropTable(
                name: "tbl_user_cursus");

            migrationBuilder.DropTable(
                name: "tbl_user_goal");

            migrationBuilder.DropTable(
                name: "tbl_user_project_members");

            migrationBuilder.DropTable(
                name: "tbl_user_project_transactions");

            migrationBuilder.DropTable(
                name: "tbl_review");

            migrationBuilder.DropTable(
                name: "tbl_spotlights");

            migrationBuilder.DropTable(
                name: "cursus");

            migrationBuilder.DropTable(
                name: "goals");

            migrationBuilder.DropTable(
                name: "tbl_user_project");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "tbl_rubric");

            migrationBuilder.DropTable(
                name: "tbl_git");

            migrationBuilder.DropTable(
                name: "tbl_user");

            migrationBuilder.DropTable(
                name: "tbl_user_details");
        }
    }
}
