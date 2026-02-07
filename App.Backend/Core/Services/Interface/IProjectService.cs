// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Query;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Models;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IProjectService : IDomainService<Project>
{
    /// <summary>
    /// Find a project by its slug.
    /// </summary>
    /// <param name="slug">The project slug.</param>
    /// <returns>The project.</returns>
    public Task<Project?> FindBySlugAsync(string slug, CancellationToken token = default);

    /// <summary>
    /// Get user projects for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>List of user projects.</returns>
    public Task<PaginatedList<UserProject>> GetUserProjectsAsync(Guid userId, ISorting sorting, IPagination pagination, CancellationToken token = default);
}
