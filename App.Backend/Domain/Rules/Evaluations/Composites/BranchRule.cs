// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Domain.Rules.Evaluations.Composites;

/// <summary>At least one child rule must pass.</summary>
public record BranchRule : Rule
{
    /// <summary>
    /// If the condition true, evaluate Then; otherwise, evaluate Else.
    /// </summary>
    public required Rule Condition { get; init; }

    /// <summary>
    /// Rules to evaluate if the condition rule passes.
    /// </summary>
    public required Rule Then { get; init; }

    /// <summary>
    /// Rules to evaluate if the condition rule fails.
    /// </summary>
    public required Rule Else { get; init; }
}
