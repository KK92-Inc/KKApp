// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Projects;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IUserProjectService : IDomainService<UserProject>
{
    /// <summary>
    /// Find a user's project session by user ID and project ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The user project session if found, null otherwise.</returns>
    Task<UserProject?> FindByUserAndProjectAsync(Guid userId, Guid projectId, CancellationToken token = default);

    /// <summary>
    /// Record a transaction for a user's project instance/session.
    /// </summary>
    /// <param name="upId"></param>
    /// <param name="userId"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    // public Task<UserProjectTransaction?> RecordAsync(Guid upId, Guid? userId, UserProjectTransactionVariant type);
}
