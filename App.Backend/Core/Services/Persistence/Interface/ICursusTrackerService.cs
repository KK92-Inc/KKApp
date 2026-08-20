// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Core.Services.Persistence.Interface;

/// <summary>
/// Re-meshes a single user's cursus snapshot against the current master track,
/// on demand, outside of a staff-triggered <see cref="CursusService.ReplaceTrackAsync"/>.
///
/// This is what lets a released branch rejoin the master track the moment a user
/// unsubscribes from a goal, instead of only on the next unrelated track edit
/// and what lets a reactivated subscription catch up on however many edits it
/// missed while inactive, in one pass.
/// </summary>
public interface ICursusSnapshot
{
    Task SyncTrackAsync(Guid userId, Guid cursusId, Guid userCursusId, CancellationToken token = default);
}
