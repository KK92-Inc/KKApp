// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Rules;

/// <summary>
/// The nested rule must NOT be satisfied (logical NOT).
/// </summary>
public sealed class NotRule : Rule
{
    /// <summary>
    /// The rule that must not be satisfied.
    /// </summary>
    [JsonPropertyName("rule")]
    public required Rule Rule { get; set; }
}
