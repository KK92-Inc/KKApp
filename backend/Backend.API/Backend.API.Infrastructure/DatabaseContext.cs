// ============================================================================
// Copyright (c) 2025 - W2Inc.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.EntityFrameworkCore;
using Backend.API.Domain.Entities.Users;

// ============================================================================

namespace Backend.API.Infrastructure;

/// <inheritdoc />
public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            optionsBuilder.EnableSensitiveDataLogging();
    }

#nullable disable
    public DbSet<User> Users { get; set; }
    public DbSet<Details> Details { get; set; }
    // public DbSet<Feature> Features { get; set; }
    // public DbSet<SpotlightEvent> SpotlightEvents { get; set; }
    // public DbSet<SpotlightEventAction> SpotlightEventsActions { get; set; }
    // public DbSet<Comment> Comments { get; set; }
    // public DbSet<Cursus> Cursi { get; set; }
    // public DbSet<Project> Projects { get; set; }
    // public DbSet<LearningGoal> LearningGoals { get; set; }
    // public DbSet<UserCursus> UserCursi { get; set; }
    // public DbSet<UserGoal> UserGoals { get; set; }
    // public DbSet<UserProject> UserProject { get; set; }
    // public DbSet<Rubric> Rubric { get; set; }
    // public DbSet<Review> Reviews { get; set; }
    // public DbSet<Git> GitInfo { get; set; }
    // public DbSet<Feedback> Feedbacks { get; set; }
    // public DbSet<Member> Members { get; set; }
    // public DbSet<Notification> Notifications { get; set; }
    // public DbSet<UserFeed> UserFeeds { get; set; }

    // // Joins
    // public DbSet<GoalProject> GoalProject { get; set; }
    // public DbSet<GoalCollaborator> GoalCollaborator { get; set; }
    // public DbSet<CursusCollaborator> CursusCollaborator { get; set; }
#nullable restore
}
