// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MemberRole
{
    /// <summary>
    /// The user is pending assignment to a role.
    /// </summary>
    [JsonPropertyName(nameof(Pending))]
    Pending,

    /// <summary>
    /// The user is a regular member of the entity.
    /// </summary>
    [JsonPropertyName(nameof(Member))]
    Member,

    /// <summary>
    /// The user is a leader of the entity, i.e., has elevated permissions.
    /// </summary>
    [JsonPropertyName(nameof(Leader))]
    Leader
}
