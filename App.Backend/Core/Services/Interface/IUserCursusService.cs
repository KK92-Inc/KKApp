// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Relations;
using App.Backend.Models.Responses.Entities.Cursus;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IUserCursusService : IDomainService<UserCursus>
{
    Task<UserCursus?> FindByUserAndCursusAsync(Guid userId, Guid cursusId, CancellationToken token = default);

    /// <summary>
    /// Loads the user's frozen snapshot and their current goal states in one shot.
    /// Returns an empty snapshot list if no snapshot exists.
    /// </summary>
    Task<(IReadOnlyList<UserCursusGoal> Snapshot, IReadOnlyDictionary<Guid, EntityObjectState> States)> GetTrackAsync(
        Guid userCursusId, Guid userId, CancellationToken token = default);

    /// <summary>
    /// Assembles the user track DO from a cursus, its snapshot, and the user's current goal states.
    /// </summary>
    UserCursusTrackDO AssembleTrack(
        Cursus cursus,
        IReadOnlyList<UserCursusGoal> snapshot,
        IReadOnlyDictionary<Guid, EntityObjectState> states);
}
