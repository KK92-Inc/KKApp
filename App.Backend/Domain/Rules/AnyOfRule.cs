// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Rules;

/// <summary>
/// At least one nested rule must be satisfied (logical OR).
/// </summary>
public sealed class AnyOfRule : Rule
{
    /// <summary>
    /// The list of rules where at least one must be satisfied.
    /// </summary>
    [JsonPropertyName("rules")]
    public required List<Rule> Rules { get; set; }
}
