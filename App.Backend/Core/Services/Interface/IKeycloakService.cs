// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Keycloak.AuthServices.Sdk.Kiota.Admin.Models;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

/// <summary>
/// Thin wrapper around the Keycloak admin client, bound to a single realm.
/// </summary>
public interface IKeycloakService
{
    /// <summary>
    /// The realm this instance operates on.
    /// </summary>
    string Realm { get; }

    #region Users

    /// <summary>
    /// Find a user by its Keycloak ID.
    /// </summary>
    /// <param name="id">The Keycloak user ID.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The user, or null if the realm has no such user.</returns>
    Task<UserRepresentation?> FindUserAsync(Guid id, CancellationToken token = default);

    /// <summary>
    /// Find a user by its exact username.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The user, or null if the realm has no such user.</returns>
    Task<UserRepresentation?> FindUserAsync(string username, CancellationToken token = default);

    /// <summary>
    /// Creates a user and returns the Keycloak-generated ID.
    /// </summary>
    /// <param name="user">The user to create. <see cref="UserRepresentation.Username"/> is required.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The ID Keycloak generated for the user.</returns>
    /// <exception cref="ServiceException">with status 409 if the username or email is taken.</exception>
    Task<Guid> CreateUserAsync(UserRepresentation user, CancellationToken token = default);

    /// <summary>
    /// Updates an existing user (PUT). Send the full representation you got from
    /// <see cref="FindUserAsync(Guid, CancellationToken)"/> with your changes applied.
    /// </summary>
    /// <param name="id">The Keycloak user ID.</param>
    /// <param name="user">The representation to store.</param>
    /// <param name="token">Cancellation token.</param>
    Task SetUserAsync(Guid id, UserRepresentation user, CancellationToken token = default);

    /// <summary>
    /// Permanently deletes a user.
    /// </summary>
    /// <param name="id">The Keycloak user ID.</param>
    /// <param name="token">Cancellation token.</param>
    Task DeleteUserAsync(Guid id, CancellationToken token = default);

    /// <summary>
    /// Enables the user so they can log in.
    /// </summary>
    /// <param name="id">The Keycloak user ID.</param>
    /// <param name="token">Cancellation token.</param>
    Task EnableUserAsync(Guid id, CancellationToken token = default);

    /// <summary>
    /// Disables the user so they can no longer log in.
    /// </summary>
    /// <param name="id">The Keycloak user ID.</param>
    /// <param name="token">Cancellation token.</param>
    Task DisableUserAsync(Guid id, CancellationToken token = default);

    #endregion

    #region Roles

    /// <summary>
    /// Assigns a realm role to the user.
    /// </summary>
    /// <param name="id">The Keycloak user ID.</param>
    /// <param name="role">The realm role name.</param>
    /// <param name="token">Cancellation token.</param>
    /// <exception cref="ServiceException">With status 404 if the role does not exist in the realm.</exception>
    Task AddRoleAsync(Guid id, string role, CancellationToken token = default);

    /// <summary>
    /// Removes a realm role from the user.
    /// </summary>
    /// <param name="id">The Keycloak user ID.</param>
    /// <param name="role">The realm role name.</param>
    /// <param name="token">Cancellation token.</param>
    /// <exception cref="ServiceException">With status 404 if the role does not exist in the realm.</exception>
    Task RemoveRoleAsync(Guid id, string role, CancellationToken token = default);

    #endregion
}