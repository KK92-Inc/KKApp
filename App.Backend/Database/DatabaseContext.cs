// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.EntityFrameworkCore;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Relations;
using App.Backend.Domain.Entities.Projects;
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
    public DbSet<ReviewRound> ReviewRounds { get; set; }
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
    public DbSet<Trial> Trials { get; set; }
    public DbSet<UserTrial> UserTrials { get; set; }

    // Joins
    public DbSet<GoalProject> GoalProject { get; set; }
    public DbSet<CursusGoal> CursusGoal { get; set; }
    public DbSet<UserCursusGoal> UserCursusGoal { get; set; }
    public DbSet<UserEvent> UserEvent { get; set; }
#nullable restore

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // A user project can have many rounds over time, but only one open at once.
        // This also makes a double-submitted "request reviews" harmless.
        modelBuilder.Entity<ReviewRound>()
            .HasIndex(r => r.UserProjectId, "IX_tbl_review_round_user_project_id_open")
            .IsUnique()
            .HasFilter("\"state\" = 0");

        // Deleting a cursus must never wipe the history of a trial that was built on it.
        modelBuilder.Entity<Trial>()
            .HasOne(t => t.Cursus)
            .WithMany()
            .HasForeignKey(t => t.CursusId)
            .OnDelete(DeleteBehavior.Restrict);

        // A user can take part in many trials over time (retaking one), but only one open at once.
        // 0 = Registered, 1 = Active, see UserTrialState. This is the backstop for the service's lock.
        modelBuilder.Entity<UserTrial>()
            .HasIndex(t => t.UserId, "IX_tbl_user_trial_user_id_open")
            .IsUnique()
            .HasFilter("\"state\" IN (0, 1)");
    }
}
