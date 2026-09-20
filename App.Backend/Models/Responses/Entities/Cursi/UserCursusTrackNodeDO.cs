// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursi;

/// <summary>One goal in a user's frozen track snapshot, with their progress on it.</summary>
public class UserCursusTrackNodeDO
{
    public required Guid GoalId { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public Guid? ParentGoalId { get; init; }

    /// <summary>Null if the user has no UserGoal row for this goal yet (hasn't started it).</summary>
    public EntityObjectState? State { get; init; }

    /// <summary>Whether the cursus's completion mode currently allows starting this goal.</summary>
    public required bool IsUnlocked { get; init; }
}
