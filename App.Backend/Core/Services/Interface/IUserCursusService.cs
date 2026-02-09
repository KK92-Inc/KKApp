// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Users;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IUserCursusService : IDomainService<UserCursus>
{
    /// <summary>
    /// Find a user's cursus enrollment by user ID and cursus ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cursusId">The cursus ID.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The user cursus enrollment if found, null otherwise.</returns>
    Task<UserCursus?> FindByUserAndCursusAsync(Guid userId, Guid cursusId, CancellationToken token = default);
}
