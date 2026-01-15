// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Core.Services;

public interface ICollaborativeService<T> where T : BaseTimestampEntity
{
    /// <summary>
    /// Adds a user to a collaborative project with a specified role.
    /// </summary>
    /// <param name="collaborative">The collaborative project entity.</param>
    /// <param name="user">The user entity to add.</param>
    /// <param name="depth">The permission depth to assign to the user within the collaborative entity.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AddCollaboratorAsync(T entity, User user, EntityPermission depth, CancellationToken token = default);

    /// <summary>
    /// Removes a user from a collaborative project.
    /// </summary>
    /// <param name="collaborative">The collaborative project entity.</param>
    /// <param name="user">The user entity to remove.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task RemoveCollaboratorAsync(T entity, User user, CancellationToken token = default);
}