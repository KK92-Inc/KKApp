// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Rules;

/// <summary>
/// User must have been registered for at least a certain number of days.
/// </summary>
public sealed class MinDaysRegisteredRule : Rule
{
    /// <summary>
    /// The minimum number of days since registration.
    /// </summary>
    [JsonPropertyName("minDays")]
    public required int MinDays { get; set; }
}
