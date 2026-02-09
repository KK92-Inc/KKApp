// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Users;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IUserGoalService : IDomainService<UserGoal>
{
    /// <summary>
    /// Find a user's goal subscription by user ID and goal ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="goalId">The goal ID.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The user goal subscription if found, null otherwise.</returns>
    Task<UserGoal?> FindByUserAndGoalAsync(Guid userId, Guid goalId, CancellationToken token = default);
}
