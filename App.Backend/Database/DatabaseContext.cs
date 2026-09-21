// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Relations;
using App.Backend.Domain.Entities.Projects;
using App.Backend.Domain;
using App.Backend.Domain.Entities.Events;

// ============================================================================

namespace App.Backend.Database;

/// <inheritdoc />
public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
#nullable disable
    public DbSet<Domain.Entities.System> System { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<SshKey> SshKeys { get; set; }
    public DbSet<Details> Details { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Cursus> Cursi { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<UserProject> UserProjects { get; set; }
    public DbSet<UserCursus> UserCursi { get; set; }
    public DbSet<UserGoal> UserGoals { get; set; }
    public DbSet<Rubric> Rubrics { get; set; }
    public DbSet<RubricVariant> RubricsVariants { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Annotation> Annotations { get; set; }
    public DbSet<GitInfo> GitInfo { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Spotlight> Spotlights { get; set; }
    public DbSet<SpotlightDismissal> SpotlightDismissals { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<UserProjectTransaction> UserProjectTransactions { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<EventFeedback> EventFeedbacks { get; set; }
    public DbSet<Kickoff> Kickoffs { get; set; }
    public DbSet<Freeze> Freezes { get; set; }

    // Joins
    public DbSet<GoalProject> GoalProject { get; set; }
    public DbSet<CursusGoal> CursusGoal { get; set; }
    public DbSet<UserCursusGoal> UserCursusGoal { get; set; }
    public DbSet<UserEvent> UserEvent { get; set; }
#nullable restore
}
