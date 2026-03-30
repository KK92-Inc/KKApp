// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Domain.Rules.Evaluations.Composites;

/// <summary>The child rule must NOT pass.</summary>
public record NotRule : Rule
{
    public required Rule Rule { get; init; }
}
