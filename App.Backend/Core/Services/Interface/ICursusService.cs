// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Relations;
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

    /// <summary>
    /// Set or replace the track (hierarchy of goals) for a fixed cursus.
    /// This is a full replacement: all existing track nodes are removed first.
    /// </summary>
    /// <param name="cursusId">The cursus ID.</param>
    /// <param name="nodes">The flat list of nodes forming the track tree.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The list of created CursusGoal entries.</returns>
    public Task<IEnumerable<CursusGoal>> SetTrackAsync(
        Guid cursusId,
        IEnumerable<CursusGoal> nodes,
        CancellationToken token = default
    );

    /// <summary>
    /// Get the track (hierarchy of goals) for a cursus.
    /// </summary>
    /// <param name="cursusId">The cursus ID.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The list of CursusGoal entries with loaded Goal navigation properties.</returns>
    public Task<IEnumerable<CursusGoal>> GetTrackAsync(Guid cursusId, CancellationToken token = default);
}
