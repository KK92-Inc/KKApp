using System;
using System.Collections.Generic;
using App.Backend.Domain.Rules.Evaluations;
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
            migrationBuilder.EnsureSchema(
                name: "internal");

            migrationBuilder.CreateTable(
                name: "system",
                schema: "internal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system", x => x.id);
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
                name: "tbl_kickoffs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    starts_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_kickoffs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descriptor = table.Column<int>(type: "integer", nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    notifiable_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data = table.Column<string>(type: "json", nullable: false),
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
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    action_text = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    href = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    background_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
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
                name: "tbl_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    login = table.Column<string>(type: "text", nullable: false),
                    display = table.Column<string>(type: "text", nullable: true),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rel_user_kickoff",
                columns: table => new
                {
                    kickoff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "tbl_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    thumbnail_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    markdown = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    threshold = table.Column<int>(type: "integer", nullable: true),
                    starts_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    closes_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    state = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_events_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_freeze",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    starts_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    invalidated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_freeze", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_freeze_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    table.ForeignKey(
                        name: "FK_tbl_ssh_key_tbl_user_user_id",
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
                    bio = table.Column<string>(type: "character varying(16384)", maxLength: 16384, nullable: true),
                    enabled_notifications = table.Column<int>(type: "integer", nullable: false),
                    github_url = table.Column<string>(type: "text", nullable: true),
                    linkedin_url = table.Column<string>(type: "text", nullable: true),
                    reddit_url = table.Column<string>(type: "text", nullable: true),
                    website_url = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "tbl_workspace",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ownership = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_workspace", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_workspace_tbl_user_owner_id",
                        column: x => x.owner_id,
                        principalTable: "tbl_user",
                        principalColumn: "id");
                });

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

            migrationBuilder.CreateTable(
                name: "tbl_event_feedback",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    comment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_event_feedback", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_event_feedback_tbl_events_event_id",
                        column: x => x.event_id,
                        principalTable: "tbl_events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_application",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    client_id = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    redirect_uris = table.Column<string[]>(type: "text[]", nullable: false),
                    scopes = table.Column<string[]>(type: "text[]", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "tbl_cursus",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    @public = table.Column<bool>(name: "public", type: "boolean", nullable: false),
                    deprecated = table.Column<bool>(type: "boolean", nullable: false),
                    variant = table.Column<int>(type: "integer", nullable: false),
                    completion = table.Column<int>(type: "integer", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_cursus", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_cursus_tbl_workspace_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "tbl_workspace",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_goals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    @public = table.Column<bool>(name: "public", type: "boolean", nullable: false),
                    deprecated = table.Column<bool>(type: "boolean", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_goals", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_goals_tbl_workspace_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "tbl_workspace",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    @public = table.Column<bool>(name: "public", type: "boolean", nullable: false),
                    deprecated = table.Column<bool>(type: "boolean", nullable: false),
                    max_members = table.Column<int>(type: "integer", nullable: false),
                    git_id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_projects", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_projects_tbl_git_git_id",
                        column: x => x.git_id,
                        principalTable: "tbl_git",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_projects_tbl_workspace_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "tbl_workspace",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_user_cursus",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    unlocks_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cursus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user_cursus", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_user_cursus_tbl_cursus_cursus_id",
                        column: x => x.cursus_id,
                        principalTable: "tbl_cursus",
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
                name: "rel_cursus_goal",
                columns: table => new
                {
                    cursus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    goal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_goal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    choice_group = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rel_cursus_goal", x => new { x.cursus_id, x.goal_id });
                    table.ForeignKey(
                        name: "FK_rel_cursus_goal_tbl_cursus_cursus_id",
                        column: x => x.cursus_id,
                        principalTable: "tbl_cursus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rel_cursus_goal_tbl_goals_goal_id",
                        column: x => x.goal_id,
                        principalTable: "tbl_goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_user_goal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    unlocks_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    goal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user_goal", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_user_goal_tbl_goals_goal_id",
                        column: x => x.goal_id,
                        principalTable: "tbl_goals",
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
                name: "rel_goal_project",
                columns: table => new
                {
                    goal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rel_goal_project", x => new { x.project_id, x.goal_id });
                    table.ForeignKey(
                        name: "FK_rel_goal_project_tbl_goals_goal_id",
                        column: x => x.goal_id,
                        principalTable: "tbl_goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rel_goal_project_tbl_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "tbl_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_rubric",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    @public = table.Column<bool>(name: "public", type: "boolean", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    deprecated = table.Column<bool>(type: "boolean", nullable: false),
                    reviewer_rules = table.Column<ICollection<Rule>>(type: "jsonb", nullable: false),
                    reviewee_rules = table.Column<ICollection<Rule>>(type: "jsonb", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    git_info_id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_rubric_tbl_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "tbl_projects",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tbl_rubric_tbl_workspace_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "tbl_workspace",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rel_cursus_goal_snapshot",
                columns: table => new
                {
                    user_cursus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    goal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_goal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    choice_group = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rel_cursus_goal_snapshot", x => new { x.user_cursus_id, x.goal_id });
                    table.ForeignKey(
                        name: "FK_rel_cursus_goal_snapshot_tbl_goals_goal_id",
                        column: x => x.goal_id,
                        principalTable: "tbl_goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rel_cursus_goal_snapshot_tbl_user_cursus_user_cursus_id",
                        column: x => x.user_cursus_id,
                        principalTable: "tbl_user_cursus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "tbl_user_project",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    unlocks_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    git_info_id = table.Column<Guid>(type: "uuid", nullable: false),
                    RubricId = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user_project", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_user_project_tbl_git_git_info_id",
                        column: x => x.git_info_id,
                        principalTable: "tbl_git",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_user_project_tbl_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "tbl_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_user_project_tbl_rubric_RubricId",
                        column: x => x.RubricId,
                        principalTable: "tbl_rubric",
                        principalColumn: "id");
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
                    @ref = table.Column<string>(name: "ref", type: "text", nullable: false),
                    scheduled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_review", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_review_tbl_rubric_rubric_id",
                        column: x => x.rubric_id,
                        principalTable: "tbl_rubric",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_review_tbl_user_project_user_project_id",
                        column: x => x.user_project_id,
                        principalTable: "tbl_user_project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_review_tbl_user_reviewer_id",
                        column: x => x.reviewer_id,
                        principalTable: "tbl_user",
                        principalColumn: "id");
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
                    table.ForeignKey(
                        name: "FK_tbl_comment_tbl_review_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "tbl_review",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tbl_comment_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rel_cursus_goal_cursus_id_parent_goal_id",
                table: "rel_cursus_goal",
                columns: new[] { "cursus_id", "parent_goal_id" });

            migrationBuilder.CreateIndex(
                name: "IX_rel_cursus_goal_goal_id",
                table: "rel_cursus_goal",
                column: "goal_id");

            migrationBuilder.CreateIndex(
                name: "IX_rel_cursus_goal_snapshot_goal_id",
                table: "rel_cursus_goal_snapshot",
                column: "goal_id");

            migrationBuilder.CreateIndex(
                name: "IX_rel_cursus_goal_snapshot_user_cursus_id_choice_group_goal_id",
                table: "rel_cursus_goal_snapshot",
                columns: new[] { "user_cursus_id", "choice_group", "goal_id" });

            migrationBuilder.CreateIndex(
                name: "IX_rel_cursus_goal_snapshot_user_cursus_id_parent_goal_id",
                table: "rel_cursus_goal_snapshot",
                columns: new[] { "user_cursus_id", "parent_goal_id" });

            migrationBuilder.CreateIndex(
                name: "IX_rel_goal_project_goal_id",
                table: "rel_goal_project",
                column: "goal_id");

            migrationBuilder.CreateIndex(
                name: "IX_rel_user_event_event_id",
                table: "rel_user_event",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_rel_user_kickoff_kickoff_id",
                table: "rel_user_kickoff",
                column: "kickoff_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_annotation_author_id",
                table: "tbl_annotation",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_annotation_review_id",
                table: "tbl_annotation",
                column: "review_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_application_workspace_id",
                table: "tbl_application",
                column: "workspace_id");

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
                name: "IX_tbl_cursus_name",
                table: "tbl_cursus",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_cursus_slug",
                table: "tbl_cursus",
                column: "slug");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_cursus_workspace_id",
                table: "tbl_cursus",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_event_feedback_event_id",
                table: "tbl_event_feedback",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_events_name",
                table: "tbl_events",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_events_user_id",
                table: "tbl_events",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_freeze_user_id",
                table: "tbl_freeze",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_git_name_owner",
                table: "tbl_git",
                columns: new[] { "name", "owner" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_goals_name",
                table: "tbl_goals",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_goals_slug",
                table: "tbl_goals",
                column: "slug");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_goals_workspace_id",
                table: "tbl_goals",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_members_git_id_user_id",
                table: "tbl_members",
                columns: new[] { "git_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_members_user_id",
                table: "tbl_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_notifications_notifiable_id_read_at",
                table: "tbl_notifications",
                columns: new[] { "notifiable_id", "read_at" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_projects_git_id",
                table: "tbl_projects",
                column: "git_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_projects_name",
                table: "tbl_projects",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_projects_slug",
                table: "tbl_projects",
                column: "slug");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_projects_workspace_id",
                table: "tbl_projects",
                column: "workspace_id");

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
                name: "IX_tbl_rubric_slug",
                table: "tbl_rubric",
                column: "slug");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_rubric_workspace_id",
                table: "tbl_rubric",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_rubric_variant_rubric_id",
                table: "tbl_rubric_variant",
                column: "rubric_id");

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
                column: "user_id",
                unique: true);

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
                name: "IX_tbl_user_project_RubricId",
                table: "tbl_user_project",
                column: "RubricId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_project_transactions_user_id",
                table: "tbl_user_project_transactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_project_transactions_user_project_id",
                table: "tbl_user_project_transactions",
                column: "user_project_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_workspace_owner_id",
                table: "tbl_workspace",
                column: "owner_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rel_cursus_goal");

            migrationBuilder.DropTable(
                name: "rel_cursus_goal_snapshot");

            migrationBuilder.DropTable(
                name: "rel_goal_project");

            migrationBuilder.DropTable(
                name: "rel_user_event");

            migrationBuilder.DropTable(
                name: "rel_user_kickoff");

            migrationBuilder.DropTable(
                name: "system",
                schema: "internal");

            migrationBuilder.DropTable(
                name: "tbl_annotation");

            migrationBuilder.DropTable(
                name: "tbl_application");

            migrationBuilder.DropTable(
                name: "tbl_comment");

            migrationBuilder.DropTable(
                name: "tbl_event_feedback");

            migrationBuilder.DropTable(
                name: "tbl_freeze");

            migrationBuilder.DropTable(
                name: "tbl_members");

            migrationBuilder.DropTable(
                name: "tbl_notifications");

            migrationBuilder.DropTable(
                name: "tbl_rubric_variant");

            migrationBuilder.DropTable(
                name: "tbl_spotlight_dismissals");

            migrationBuilder.DropTable(
                name: "tbl_ssh_key");

            migrationBuilder.DropTable(
                name: "tbl_user_details");

            migrationBuilder.DropTable(
                name: "tbl_user_goal");

            migrationBuilder.DropTable(
                name: "tbl_user_project_transactions");

            migrationBuilder.DropTable(
                name: "tbl_user_cursus");

            migrationBuilder.DropTable(
                name: "tbl_kickoffs");

            migrationBuilder.DropTable(
                name: "tbl_review");

            migrationBuilder.DropTable(
                name: "tbl_events");

            migrationBuilder.DropTable(
                name: "tbl_spotlights");

            migrationBuilder.DropTable(
                name: "tbl_goals");

            migrationBuilder.DropTable(
                name: "tbl_cursus");

            migrationBuilder.DropTable(
                name: "tbl_user_project");

            migrationBuilder.DropTable(
                name: "tbl_rubric");

            migrationBuilder.DropTable(
                name: "tbl_projects");

            migrationBuilder.DropTable(
                name: "tbl_git");

            migrationBuilder.DropTable(
                name: "tbl_workspace");

            migrationBuilder.DropTable(
                name: "tbl_user");
        }
    }
}
