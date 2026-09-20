// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Relations;
using App.Backend.Models;
using App.Backend.Models.Responses.Entities.Cursi;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface ICursusService : IDomainService<Cursus>, ISlugQueryable<Cursus>
{
    /// <summary>
    /// Validates that every goal referenced in a proposed track actually exists.
    /// Structural validity (duplicates, cycles, depth, fan-out) is already guaranteed
    /// by <see cref="Models.Requests.Cursus.PutCursusTrackRequestDTO"/>'s own
    /// IValidatableObject implementation by the time this runs - this only checks
    /// what the DTO can't: whether the referenced goals are real.
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task ValidateTrackAsync(IReadOnlyList<(Guid GoalId, Guid? ParentId)> nodes, CancellationToken token = default);

    /// <summary>
    /// Fully replaces the track for a static cursus, removing all existing nodes first.
    /// Existing UserCursusGoal snapshots are untouched - a track edit only affects users
    /// who subscribe from this point forward. Returns the persisted nodes with Goal
    /// navigation properties loaded.
    /// </summary>
    Task<IReadOnlyList<CursusGoal>> SetTrackAsync(Guid cursusId, IEnumerable<CursusGoal> nodes, CancellationToken token = default);

    /// <summary>
    /// Returns all track nodes for a cursus with Goal navigation properties loaded.
    /// </summary>
    Task<IReadOnlyList<CursusGoal>> GetTrackAsync(Guid cursusId, CancellationToken token = default);

    /// <summary>Assembles the master-track DO from a cursus and its track nodes.</summary>
    CursusTrackDO AssembleTrack(Cursus cursus, IReadOnlyList<CursusGoal> nodes);
}
