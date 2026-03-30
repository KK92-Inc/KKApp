// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Domain.Rules.Evaluations;

public record HasGoalRule : Rule
{
    public required Guid GoalId { get; init; }
}
