// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursus;

/// <summary>
/// A single node in a user's cursus track, enriched with progression state.
/// </summary>
public record UserCursusTrackNodeDO(
    Guid GoalId,
    string Name,
    string Slug,
    Guid? ParentGoalId,
    Guid? ChoiceGroup,

    /// <summary>
    /// The user's current state for this goal. Null means not yet subscribed.
    /// </summary>
    EntityObjectState? State,

    /// <summary>
    /// Whether the prerequisites for this goal are satisfied under the cursus
    /// completion mode. Unlocked does not mean subscribed — just accessible.
    /// </summary>
    bool IsUnlocked
);
