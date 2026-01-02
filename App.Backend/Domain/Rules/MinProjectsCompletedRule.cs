// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Rules;

/// <summary>
/// User must have completed at least a minimum number of projects.
/// </summary>
public sealed class MinProjectsCompletedRule : Rule
{
    /// <summary>
    /// The minimum number of projects the user must have completed.
    /// </summary>
    [JsonPropertyName("minCount")]
    public required int MinCount { get; set; }
}
