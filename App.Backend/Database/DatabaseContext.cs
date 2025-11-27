// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.EntityFrameworkCore;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Reviews;

// ============================================================================

namespace App.Backend.Database;

/// <inheritdoc />
public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
#nullable disable
    public DbSet<User> Users { get; set; }
    public DbSet<Details> Details { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Cursus> Cursi { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<UserProject> UserProject { get; set; }
    public DbSet<UserCursus> UserCursi { get; set; }
    public DbSet<UserGoal> UserGoals { get; set; }
    public DbSet<Rubric> Rubric { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Git> GitInfo { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    // // Joins
    // public DbSet<GoalProject> GoalProject { get; set; }
    // public DbSet<GoalCollaborator> GoalCollaborator { get; set; }
    // public DbSet<CursusCollaborator> CursusCollaborator { get; set; }
#nullable restore
}
