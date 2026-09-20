// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursi;

/// <summary>
/// A cursus's official track - the live master, not any one user's snapshot. Built by
/// <see cref="Core.Services.Interface.ICursusService.AssembleTrack"/>.
/// </summary>
public class CursusTrackDO
{
    public required Guid CursusId { get; init; }
    public required string Name { get; init; }
    public required CursusMode CompletionMode { get; init; }
    public required IReadOnlyList<CursusTrackNodeDO> Nodes { get; init; }
}
