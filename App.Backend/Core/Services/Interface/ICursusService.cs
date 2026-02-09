// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Relations;
using App.Backend.Models;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface ICursusService : IDomainService<Cursus>, ISlugQueryable<Cursus>
{
    /// <summary>
    /// Set or replace the track (hierarchy of goals) for a fixed cursus.
    /// This is a full replacement: all existing track nodes are removed first.
    /// </summary>
    /// <param name="cursusId">The cursus ID.</param>
    /// <param name="nodes">The flat list of nodes forming the track tree.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The list of created CursusGoal entries.</returns>
    public Task<IEnumerable<CursusGoal>> SetTrackAsync(Guid cursusId, IEnumerable<CursusGoal> nodes, CancellationToken token = default);

    /// <summary>
    /// Get the track (hierarchy of goals) for a cursus.
    /// </summary>
    /// <param name="cursusId">The cursus ID.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The list of CursusGoal entries with loaded Goal navigation properties.</returns>
    public Task<IEnumerable<CursusGoal>> GetTrackAsync(Guid cursusId, CancellationToken token = default);

    /// <summary>
    /// Compute the cursus track for a user
    /// </summary>
    /// <param name="cursusId">The cursus ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="token">Cancellation token.</param>
    public Task<IReadOnlyDictionary<Guid, EntityObjectState>> GetTrackForUserAsync(Guid cursusId, Guid userId, CancellationToken token = default);
}
