// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Rules;

/// <summary>
/// User must have completed at least a minimum number of reviews.
/// </summary>
public sealed class MinReviewsCompletedRule : Rule
{
    /// <summary>
    /// The minimum number of reviews the user must have completed.
    /// </summary>
    [JsonPropertyName("minCount")]
    public required int MinCount { get; set; }
}
