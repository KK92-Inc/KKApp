// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursi;

/// <summary>
/// A user's frozen track snapshot, taken once at subscribe time - see
/// <see cref="Core.Services.Implementation.SubscriptionService.SubscribeToCursusAsync"/>.
/// Later edits to the cursus's master track never change this; it reflects the track
/// exactly as it stood the moment the user enrolled. Built by
/// <see cref="Core.Services.Interface.IUserCursusService.AssembleTrack"/>.
/// </summary>
public class UserCursusTrackDO
{
    public required Guid CursusId { get; init; }
    public required string Name { get; init; }
    public required CursusMode CompletionMode { get; init; }
    public required IReadOnlyList<UserCursusTrackNodeDO> Nodes { get; init; }
}
