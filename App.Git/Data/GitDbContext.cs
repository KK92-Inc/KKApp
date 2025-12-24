// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Git.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Git.Data;

/// <summary>
/// Database context for the Git server.
/// </summary>
public class GitDbContext(DbContextOptions<GitDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<SshKey> SshKeys => Set<SshKey>();
    public DbSet<Repository> Repositories => Set<Repository>();
    public DbSet<Collaborator> Collaborators => Set<Collaborator>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.ExternalId);
        });

        // SshKey configuration
        modelBuilder.Entity<SshKey>(entity =>
        {
            entity.HasIndex(e => e.Fingerprint).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany(u => u.SshKeys)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Repository configuration
        modelBuilder.Entity<Repository>(entity =>
        {
            entity.HasIndex(e => new { e.OwnerId, e.Name }).IsUnique();
            entity.HasOne(e => e.Owner)
                  .WithMany(u => u.OwnedRepositories)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Collaborator configuration
        modelBuilder.Entity<Collaborator>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.RepositoryId }).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Collaborations)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Repository)
                  .WithMany(r => r.Collaborators)
                  .HasForeignKey(e => e.RepositoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }
    }
}
