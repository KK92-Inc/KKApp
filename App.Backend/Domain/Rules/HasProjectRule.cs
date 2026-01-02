// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Rules;

/// <summary>
/// User must have completed a specific project.
/// </summary>
public sealed class HasProjectRule : Rule
{
    /// <summary>
    /// The slug of the project that must be completed.
    /// </summary>
    [JsonPropertyName("project_id")]
    public required Guid ProjectId { get; set; }
    
}
