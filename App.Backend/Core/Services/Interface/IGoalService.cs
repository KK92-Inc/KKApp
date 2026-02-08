// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Query;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Models;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IGoalService : IDomainService<Goal>, ISlugQueryable<Goal>
{
    /// <summary>
    /// Get projects associated with a goal.
    /// </summary>
    /// <param name="goalId">The goal ID.</param>
    /// <returns>List of projects.</returns>
    public Task<Goal> SetProjectsAsync(Guid goalId, IEnumerable<Guid> projects, CancellationToken token = default);

    public Task<IEnumerable<Project>> GetProjectsAsync(Guid goalId, CancellationToken token = default);

    /// <summary>
    /// Get projects associated with a goal.
    /// </summary>
    /// <param name="goalId">The goal ID.</param>
    /// <returns>List of projects.</returns>
    public Task<PaginatedList<UserGoal>> GetUsersAsync(
        Guid goalId,
        ISorting sorting,
        IPagination pagination,
        CancellationToken token = default
    );
}
