// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Domain.Rules.Evaluations;

// ── Leaf rules ─────────────────────────────────────────────────────────────────

public record HasCursusRule : Rule
{
    public required Guid CursusId { get; init; }
}
