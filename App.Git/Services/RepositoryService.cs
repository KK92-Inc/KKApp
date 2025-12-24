// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Git.Data;
using App.Git.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Git.Services;

/// <summary>
/// Service for repository management.
/// </summary>
public class RepositoryService
{
    private readonly GitDbContext _db;
    private readonly GitService _git;
    private readonly ILogger<RepositoryService> _logger;

    public RepositoryService(GitDbContext db, GitService git, ILogger<RepositoryService> logger)
    {
        _db = db;
        _git = git;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new repository.
    /// </summary>
    public async Task<Repository> CreateAsync(Guid ownerId, string name, string? description = null, RepositoryVisibility visibility = RepositoryVisibility.Private)
    {
        // Validate owner exists
        var owner = await _db.Users.FindAsync(ownerId);
        if (owner == null)
        {
            throw new InvalidOperationException("Owner not found");
        }

        // Check for duplicate name
        var existing = await _db.Repositories.FirstOrDefaultAsync(r => r.OwnerId == ownerId && r.Name == name);
        if (existing != null)
        {
            throw new InvalidOperationException($"Repository '{name}' already exists for this user");
        }

        // Create on disk
        var path = _git.CreateRepository(owner.Username, name);

        // Create in database
        var repo = new Repository
        {
            OwnerId = ownerId,
            Name = name,
            Description = description,
            Path = path,
            Visibility = visibility
        };

        _db.Repositories.Add(repo);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created repository {Owner}/{Name}", owner.Username, name);
        return repo;
    }

    /// <summary>
    /// Gets a repository by owner and name.
    /// </summary>
    public async Task<Repository?> GetAsync(string owner, string name)
    {
        return await _db.Repositories
            .Include(r => r.Owner)
            .FirstOrDefaultAsync(r => r.Owner!.Username == owner && r.Name == name);
    }

    /// <summary>
    /// Gets a repository by ID.
    /// </summary>
    public async Task<Repository?> GetByIdAsync(Guid id)
    {
        return await _db.Repositories
            .Include(r => r.Owner)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    /// <summary>
    /// Lists repositories accessible to a user.
    /// </summary>
    public async Task<List<Repository>> ListAsync(Guid? userId = null, int skip = 0, int take = 30)
    {
        var query = _db.Repositories.Include(r => r.Owner).AsQueryable();

        if (userId.HasValue)
        {
            // User can see:
            // 1. Their own repos
            // 2. Repos they collaborate on
            // 3. Public/Internal repos
            query = query.Where(r =>
                r.OwnerId == userId.Value ||
                r.Collaborators.Any(c => c.UserId == userId.Value) ||
                r.Visibility == RepositoryVisibility.Public ||
                r.Visibility == RepositoryVisibility.Internal
            );
        }
        else
        {
            // Anonymous users can only see public repos
            query = query.Where(r => r.Visibility == RepositoryVisibility.Public);
        }

        return await query
            .OrderByDescending(r => r.UpdatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    /// <summary>
    /// Lists repositories owned by a user.
    /// </summary>
    public async Task<List<Repository>> ListByOwnerAsync(Guid ownerId, int skip = 0, int take = 30)
    {
        return await _db.Repositories
            .Include(r => r.Owner)
            .Where(r => r.OwnerId == ownerId)
            .OrderByDescending(r => r.UpdatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    /// <summary>
    /// Updates repository settings.
    /// </summary>
    public async Task<Repository?> UpdateAsync(Guid repoId, string? description = null, RepositoryVisibility? visibility = null, bool? isArchived = null)
    {
        var repo = await _db.Repositories.FindAsync(repoId);
        if (repo == null)
        {
            return null;
        }

        if (description != null) repo.Description = description;
        if (visibility.HasValue) repo.Visibility = visibility.Value;
        if (isArchived.HasValue) repo.IsArchived = isArchived.Value;

        await _db.SaveChangesAsync();
        return repo;
    }

    /// <summary>
    /// Deletes a repository.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid repoId)
    {
        var repo = await _db.Repositories.FindAsync(repoId);
        if (repo == null)
        {
            return false;
        }

        // Delete from disk
        _git.DeleteRepository(repo.Path);

        // Delete from database
        _db.Repositories.Remove(repo);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted repository {RepoId}", repoId);
        return true;
    }

    /// <summary>
    /// Checks if a user has permission to access a repository.
    /// </summary>
    public async Task<bool> HasAccessAsync(Guid repoId, Guid? userId, CollaboratorPermission requiredPermission = CollaboratorPermission.Read)
    {
        var repo = await _db.Repositories
            .Include(r => r.Collaborators)
            .FirstOrDefaultAsync(r => r.Id == repoId);

        if (repo == null) return false;

        // Public repos are readable by everyone
        if (repo.Visibility == RepositoryVisibility.Public && requiredPermission == CollaboratorPermission.Read)
        {
            return true;
        }

        // Must be authenticated for anything else
        if (!userId.HasValue) return false;

        // Owner has full access
        if (repo.OwnerId == userId.Value) return true;

        // Check collaborator permission
        var collab = repo.Collaborators.FirstOrDefault(c => c.UserId == userId.Value);
        if (collab == null) return false;

        return collab.Permission >= requiredPermission;
    }
}
