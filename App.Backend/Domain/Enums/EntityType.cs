// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EntityType
{
    /// <summary>
    /// A project entity.
    /// </summary>
    [JsonPropertyName(nameof(Project))]
    Project = 1,

    /// <summary>
    /// A Goal entity.
    /// </summary>
    [JsonPropertyName(nameof(Goal))]
    Goal = 3,

    /// <summary>
    /// A Cursus entity.
    /// </summary>
    [JsonPropertyName(nameof(Cursus))]
    Cursus = 4,

    /// <summary>
    /// A Rubric entity.
    /// </summary>
    [JsonPropertyName(nameof(Rubric))]
    Rubric = 5,
}

