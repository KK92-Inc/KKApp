// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Git.Data;
using App.Git.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Git.Services;

/// <summary>
/// Service for managing collaborators.
/// </summary>
public class CollaboratorService
{
    private readonly GitDbContext _db;
    private readonly ILogger<CollaboratorService> _logger;

    public CollaboratorService(GitDbContext db, ILogger<CollaboratorService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Adds a collaborator to a repository.
    /// </summary>
    public async Task<Collaborator> AddAsync(Guid repoId, Guid userId, CollaboratorPermission permission = CollaboratorPermission.Read)
    {
        // Validate repository exists
        var repo = await _db.Repositories.FindAsync(repoId);
        if (repo == null)
        {
            throw new InvalidOperationException("Repository not found");
        }

        // Can't add owner as collaborator
        if (repo.OwnerId == userId)
        {
            throw new InvalidOperationException("Owner cannot be added as a collaborator");
        }

        // Validate user exists
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Check for existing collaboration
        var existing = await _db.Collaborators.FirstOrDefaultAsync(c => c.RepositoryId == repoId && c.UserId == userId);
        if (existing != null)
        {
            // Update permission instead
            existing.Permission = permission;
            await _db.SaveChangesAsync();
            return existing;
        }

        var collab = new Collaborator
        {
            RepositoryId = repoId,
            UserId = userId,
            Permission = permission
        };

        _db.Collaborators.Add(collab);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Added collaborator {UserId} to repository {RepoId} with permission {Permission}",
            userId, repoId, permission);
        return collab;
    }

    /// <summary>
    /// Removes a collaborator from a repository.
    /// </summary>
    public async Task<bool> RemoveAsync(Guid repoId, Guid userId)
    {
        var collab = await _db.Collaborators.FirstOrDefaultAsync(c => c.RepositoryId == repoId && c.UserId == userId);
        if (collab == null)
        {
            return false;
        }

        _db.Collaborators.Remove(collab);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Removed collaborator {UserId} from repository {RepoId}", userId, repoId);
        return true;
    }

    /// <summary>
    /// Lists all collaborators for a repository.
    /// </summary>
    public async Task<List<Collaborator>> ListAsync(Guid repoId)
    {
        return await _db.Collaborators
            .Include(c => c.User)
            .Where(c => c.RepositoryId == repoId)
            .OrderBy(c => c.User!.Username)
            .ToListAsync();
    }

    /// <summary>
    /// Updates a collaborator's permission.
    /// </summary>
    public async Task<Collaborator?> UpdatePermissionAsync(Guid repoId, Guid userId, CollaboratorPermission permission)
    {
        var collab = await _db.Collaborators.FirstOrDefaultAsync(c => c.RepositoryId == repoId && c.UserId == userId);
        if (collab == null)
        {
            return null;
        }

        collab.Permission = permission;
        await _db.SaveChangesAsync();
        return collab;
    }
}
