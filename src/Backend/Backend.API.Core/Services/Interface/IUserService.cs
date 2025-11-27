// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Linq.Dynamic.Core;
using Backend.API.Domain.Entities.Users;

// ============================================================================

namespace Backend.API.Core.Services.Interface;

public interface IUserService : IDomainService<User>
{
    /// <summary>
    /// Find the user by its login.
    /// </summary>
    /// <param name="login">The login.</param>
    /// <returns>The user.</returns>
    public Task<User?> FindByLoginAsync(string login);

    /// <summary>
    /// Find the user by its display name.
    /// </summary>
    /// <param name="displayName">The Display name</param>
    /// <returns>The user.</returns>
    public Task<User?> FindByNameAsync(string displayName);

    // public override async Task<PagedResult>
}
