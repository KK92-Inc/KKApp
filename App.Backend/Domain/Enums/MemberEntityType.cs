// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Enums;

/// <summary>
/// Members is a generic association entity that links users to various other
/// entities in the system (e.g. projects, rubrics).
/// 
/// This enum defines the types of entities that a member can be associated with.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MemberEntityType
{
    /// <summary>
    /// The entity describes a workspace.
    /// </summary>
    [JsonPropertyName(nameof(Workspace))]
    Workspace = 1,

    /// <summary>
    /// The entity describes a user's session in a user project
    /// </summary>
    [JsonPropertyName(nameof(UserProject))]
    UserProject = 2,
}

