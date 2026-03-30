// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Domain.Rules.Evaluations.Composites;

// ── Composite rules ────────────────────────────────────────────────────────────

/// <summary>All child rules must pass.</summary>
public record AllOfRule : Rule
{
    public required IReadOnlyList<Rule> Rules { get; init; }
}
