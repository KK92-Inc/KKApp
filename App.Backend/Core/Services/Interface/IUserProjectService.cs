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
    /// Record a transaction for a user's project instance/session.
    /// </summary>
    /// <param name="upId"></param>
    /// <param name="userId"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    // public Task<UserProjectTransaction?> RecordAsync(Guid upId, Guid? userId, UserProjectTransactionVariant type);
}
