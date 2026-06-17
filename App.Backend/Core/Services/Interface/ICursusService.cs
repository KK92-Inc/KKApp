// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Relations;
using App.Backend.Models;
using App.Backend.Models.Responses.Entities.Cursus;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface ICursusService : IDomainService<Cursus>, ISlugQueryable<Cursus>
{
    /// <summary>
    /// Validates the structural integrity of a track payload.
    /// Returns an error message on failure, null on success.
    /// </summary>
    Task<string?> ValidateTrackAsync(
        IReadOnlyList<(Guid GoalId, Guid? ParentId, Guid? Group)> nodes,
        CancellationToken token = default);

    /// <summary>
    /// Fully replaces the track for a static cursus, removing all existing nodes first.
    /// Returns the persisted nodes with Goal navigation properties loaded.
    /// </summary>
    Task<IReadOnlyList<CursusGoal>> ReplaceTrackAsync(
        Guid cursusId,
        IEnumerable<CursusGoal> nodes,
        CancellationToken token = default);

    /// <summary>
    /// Returns all track nodes for a cursus with Goal navigation properties loaded.
    /// </summary>
    Task<IReadOnlyList<CursusGoal>> GetTrackAsync(Guid cursusId, CancellationToken token = default);

    /// <summary>
    /// Assembles the track response DO from a cursus and its loaded goal relations.
    /// </summary>
    CursusTrackDO AssembleTrack(Cursus cursus, IReadOnlyList<CursusGoal> goals);
}
