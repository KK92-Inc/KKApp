// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure JsonDocument value converter for InMemory/SQLite compatibility
        var jsonDocumentConverter = new ValueConverter<JsonDocument?, string?>(
            v => v == null ? null : v.RootElement.GetRawText(),
            v => v == null ? null : JsonDocument.Parse(v, default)
        );

        var jsonDocumentRequiredConverter = new ValueConverter<JsonDocument, string>(
            v => v.RootElement.GetRawText(),
            v => JsonDocument.Parse(v, default)
        );

        modelBuilder.Entity<Cursus>()
            .Property(c => c.Track)
            .HasConversion(jsonDocumentConverter);

        modelBuilder.Entity<Notification>()
            .Property(n => n.Data)
            .HasConversion(jsonDocumentRequiredConverter);

        // modelBuilder.Entity<Notification>()
        //     .Property(p => p.Data)
        //     .HasColumnType("jsonb")
        //     .IsRequired();
    }

#nullable disable
    public DbSet<User> Users { get; set; }
    public DbSet<Details> Details { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Cursus> Cursi { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<UserProject> UserProjects { get; set; }
    public DbSet<UserCursus> UserCursi { get; set; }
    public DbSet<UserGoal> UserGoals { get; set; }
    public DbSet<Rubric> Rubric { get; set; }
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
