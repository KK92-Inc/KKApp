// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Relations;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IUserCursusService : IDomainService<UserCursus>
{
    /// <summary>
    /// Find a user's cursus enrollment by user ID and cursus ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cursusId">The cursus ID.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The user cursus enrollment if found, null otherwise.</returns>
    Task<UserCursus?> FindByUserAndCursusAsync(Guid userId, Guid cursusId, CancellationToken token = default);

    /// <summary>
    /// Advances the user's track for a given cursus based on their completed goals.
    /// This method should be called whenever a user completes a goal, to ensure
    /// their track is up to date. It will unlock new goals in the user's snapshot
    /// according to the completion mode of the cursus (FreeStyle or Ring).
    /// 
    /// It basically does frontier expansion on the user's snapshot of the cursus track.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="userCursusId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task AdvanceTrackAsync(Guid userId, Guid userCursusId, CancellationToken token = default);

    public Task<IReadOnlyList<UserCursusGoal>> GetSnapshotAsync(Guid userCursusId, CancellationToken token = default);

    public Task<IReadOnlyDictionary<Guid, EntityObjectState>> GetSnapshotStatesAsync(
        Guid userId,
        IEnumerable<Guid> goalIds,
        CancellationToken token = default);


}
