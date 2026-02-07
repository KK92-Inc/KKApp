// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Models;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface ICursusService : IDomainService<Cursus>
{
    /// <summary>
    /// Find a goal by its slug.
    /// </summary>
    /// <param name="slug">The goal slug.</param>
    /// <returns>The goal.</returns>
    public Task<Cursus?> FindBySlugAsync(string slug);

    /// <summary>
    /// Get users associated with a cursus.
    /// </summary>
    /// <param name="cursusId">The cursus ID.</param>
    /// <returns>List of users.</returns>
    public Task<IEnumerable<User>> GetCursusUsers(Guid cursusId);

    /// <summary>
    /// Get goals associated with a cursus.
    /// </summary>
    /// <param name="goalId">The goal ID.</param>
    /// <returns>List of projects.</returns>
    public Task<IEnumerable<Project>> GetCursusGoals(Guid goalId);

    /// <summary>
    /// Get projects associated with a cursus.
    /// </summary>
    /// <param name="cursusId">The cursus ID.</param>
    /// <returns>List of projects.</returns>
    public Task<IEnumerable<Project>> GetCursusProjects(Guid cursusId);
}
