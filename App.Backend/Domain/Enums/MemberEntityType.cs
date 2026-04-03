// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MemberEntityType
{
    /// <summary>
    /// The entity describes a rubric.
    /// </summary>
    [JsonPropertyName(nameof(Rubric))]
    Rubric = 0,

    /// <summary>
    /// The entity describes a project.
    /// </summary>
    [JsonPropertyName(nameof(Project))]
    Project = 1,

    /// <summary>
    /// The entity describes a user's session in a project.
    /// </summary>
    [JsonPropertyName(nameof(UserProject))]
    UserProject = 2,

    /// <summary>
    /// The entity describes a goal, which is part of a cursus.
    /// </summary>
    [JsonPropertyName(nameof(Goal))]
    Goal = 3,

    /// <summary>
    /// The entity describes a cursus, which is a collection of goals.
    /// </summary>
    [JsonPropertyName(nameof(Cursus))]
    Cursus = 4
}

