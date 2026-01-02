// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Rules;

/// <summary>
/// All nested rules must be satisfied (logical AND).
/// </summary>
public sealed class AllOfRule : Rule
{
    /// <summary>
    /// The list of rules that must all be satisfied.
    /// </summary>
    [JsonPropertyName("rules")]
    public required List<Rule> Rules { get; set; }
}
