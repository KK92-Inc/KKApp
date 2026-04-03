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

// ============================================================================

namespace App.Backend.Database;

/// <inheritdoc />
public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     optionsBuilder.UseAsyncSeeding(async (context, _, cancellationToken) =>
    //     {
    //         // var testBlog = await context.Set<Blog>().FirstOrDefaultAsync(b => b.Url == "http://test.com", cancellationToken);
    //         // if (testBlog == null)
    //         // {
    //         //     context.Set<Blog>().Add(new Blog { Url = "http://test.com" });
    //         //     await context.SaveChangesAsync(cancellationToken);
    //         // }
    //     });
    // }

#nullable disable
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
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Git> GitInfo { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Spotlight> Spotlights { get; set; }
    public DbSet<SpotlightDismissal> SpotlightDismissals { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<UserProjectTransaction> UserProjectTransactions { get; set; }

    // Joins
    public DbSet<GoalProject> GoalProject { get; set; }
    public DbSet<CursusGoal> CursusGoal { get; set; }
    // public DbSet<GoalCollaborator> GoalCollaborator { get; set; }
    // public DbSet<CursusCollaborator> CursusCollaborator { get; set; }
#nullable restore

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Rule collections are stored as jsonb. Register value converters so
        // EF Core serializes them via System.Text.Json (which supports the
        // [JsonPolymorphic] attributes on Rule) rather than delegating to Npgsql,
        // which requires an explicit dynamic-JSON opt-in for interface types.
        var jsonOptions = new JsonSerializerOptions();

        modelBuilder.Entity<Rubric>()
            .Property(r => r.ReviewerRules)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<ICollection<Rule>>(v, jsonOptions) ?? new List<Rule>()
            );

        modelBuilder.Entity<Rubric>()
            .Property(r => r.RevieweeRules)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<ICollection<Rule>>(v, jsonOptions) ?? new List<Rule>()
            );
    }
}
