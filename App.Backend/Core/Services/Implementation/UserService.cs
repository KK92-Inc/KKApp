// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class UserService(DatabaseContext ctx) : BaseService<User>(ctx), IUserService
{
    private readonly DatabaseContext _context = ctx;

    public async Task<User?> FindByLoginAsync(string login, CancellationToken token = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Login == login, cancellationToken: token);
    }

    public async Task<User?> FindByNameAsync(string displayName, CancellationToken token = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Display == displayName, cancellationToken: token);
    }

    /// <summary>
    /// Add an SSH key to a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="sshKey">The SSH key to add.</param>
    /// <param name="token">Cancellation token.</param>
    public async Task AddSshKeyAsync(Guid userId, SshKey sshKey, CancellationToken token = default)
    {
        sshKey.UserId = userId;
        await _context.SshKeys.AddAsync(sshKey, token);
        await _context.SaveChangesAsync(token);
    }

    /// <summary>
    /// Remove an SSH key from a user.
    /// </summary>
    /// <param name="fingerprint">The SSH key fingerprint to remove.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>True if removed, false if not found.</returns>
    public async Task<bool> RemoveSshKeyAsync(string fingerprint, CancellationToken token = default)
    {
        var key = await _context.SshKeys.FirstOrDefaultAsync(k => k.Fingerprint == fingerprint, token);
        if (key is null)
            return false;

        _context.SshKeys.Remove(key);
        await _context.SaveChangesAsync(token);
        return true;
    }

    /// <summary>
    /// Get all SSH keys for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="token">Cancellation token.</param>
    public async Task<IEnumerable<SshKey>> GetSshKeysAsync(Guid userId, CancellationToken token = default)
    {
        return await _context.Set<SshKey>()
            .Where(k => k.UserId == userId)
            .ToListAsync(token);
    }
}
