// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Domain.Rules.Evaluations.Composites;

/// <summary>At least one child rule must pass.</summary>
public record AnyOfRule : Rule
{
    public required IReadOnlyList<Rule> Rules { get; init; }
}
