// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserProjectRole
{
    /// <summary>
    /// The user is pending assignment to a role.
    /// </summary>
    [JsonPropertyName(nameof(Pending))]
    Pending,

    /// <summary>
    /// The user is a regular member of the project.
    /// </summary>
    [JsonPropertyName(nameof(Member))]
    Member,

    /// <summary>
    /// The user is a leader of the project session.
    /// </summary>
    [JsonPropertyName(nameof(Leader))]
    Leader
}
