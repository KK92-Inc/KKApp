// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Rules;

/// <summary>
/// User must be in the same timezone as the reviewee.
/// Useful for synchronous peer reviews.
/// </summary>
public sealed class SameTimezoneRule : Rule
{
    /// <summary>
    /// Maximum hour difference allowed. Default is 0 (exact match).
    /// </summary>
    [JsonPropertyName("maxHourDifference")]
    public int MaxHourDifference { get; set; } = 0;
}
