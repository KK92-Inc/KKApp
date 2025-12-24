// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Git.Data;
using App.Git.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Git.Services;

/// <summary>
/// Service for user management.
/// </summary>
public class UserService
{
    private readonly GitDbContext _db;
    private readonly ILogger<UserService> _logger;

    public UserService(GitDbContext db, ILogger<UserService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Gets or creates a user from an external identity.
    /// </summary>
    public async Task<User> GetOrCreateFromExternalAsync(string externalId, string username, string? email = null)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.ExternalId == externalId);
        if (user != null)
        {
            // Update username/email if changed
            if (user.Username != username || user.Email != email)
            {
                user.Username = username;
                user.Email = email;
                await _db.SaveChangesAsync();
            }
            return user;
        }

        // Create new user
        user = new User
        {
            ExternalId = externalId,
            Username = username,
            Email = email
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created user {Username} from external ID {ExternalId}", username, externalId);
        return user;
    }

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _db.Users.FindAsync(id);
    }

    /// <summary>
    /// Gets a user by username.
    /// </summary>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    /// <summary>
    /// Lists all users.
    /// </summary>
    public async Task<List<User>> ListAsync(int skip = 0, int take = 30)
    {
        return await _db.Users
            .OrderBy(u => u.Username)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
}
