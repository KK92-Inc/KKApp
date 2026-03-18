// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Linq.Dynamic.Core;
using App.Backend.Domain.Entities.Users;
using App.Backend.Models;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IUserService : IDomainService<User>
{
    /// <summary>
    /// Find the user by its login.
    /// </summary>
    /// <param name="login">The login.</param>
    /// <returns>The user.</returns>
    public Task<User?> FindByLoginAsync(string login, CancellationToken token = default);

    /// <summary>
    /// Find the user by its display name.
    /// </summary>
    /// <param name="displayName">The Display name</param>
    /// <returns>The user.</returns>
    public Task<User?> FindByNameAsync(string displayName, CancellationToken token = default);

    /// <summary>
    /// Add an SSH key to a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="sshKey">The SSH key to add.</param>
    /// <param name="token">Cancellation token.</param>
    public Task AddSshKeyAsync(Guid userId, SshKey sshKey, CancellationToken token = default);

    /// <summary>
    /// Remove an SSH key from a user.
    /// </summary>
    /// <param name="fingerprint">The SSH key fingerprint to remove.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>True if removed, false if not found.</returns>
    public Task<bool> RemoveSshKeyAsync(string fingerprint, CancellationToken token = default);

    /// <summary>
    /// Get all SSH keys for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="token">Cancellation token.</param>
    public Task<IEnumerable<SshKey>> GetSshKeysAsync(Guid userId, CancellationToken token = default);
}
