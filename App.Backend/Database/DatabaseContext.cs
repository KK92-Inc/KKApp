// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.EntityFrameworkCore;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Relations;
using App.Backend.Domain.Entities.Projects;

// ============================================================================

namespace App.Backend.Database;

/// <inheritdoc />
public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{

#nullable disable
    public DbSet<User> Users { get; set; }
    public DbSet<SshKey> SshKeys { get; set; }
    public DbSet<Details> Details { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Cursus> Cursi { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<UserProject> UserProjects { get; set; }
    public DbSet<UserCursus> UserCursi { get; set; }
    public DbSet<UserGoal> UserGoals { get; set; }
    public DbSet<Rubric> Rubrics { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Git> GitInfo { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Spotlight> Spotlights { get; set; }
    public DbSet<SpotlightDismissal> SpotlightDismissals { get; set; }
    public DbSet<UserProjectMember> UserProjectMembers { get; set; }
    public DbSet<UserProjectTransaction> UserProjectTransactions { get; set; }

    // Joins
    public DbSet<GoalProject> GoalProject { get; set; }
    // public DbSet<GoalCollaborator> GoalCollaborator { get; set; }
    // public DbSet<CursusCollaborator> CursusCollaborator { get; set; }
#nullable restore
}
