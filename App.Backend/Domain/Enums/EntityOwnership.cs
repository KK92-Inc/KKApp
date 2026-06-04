// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Enums;

/// <summary>
/// Describes the ownership type of an entity, for example:
/// - A workspace can be owned by a user or an organization.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EntityOwnership
{
    /// <summary>
    /// The entity is owned by a user.
    /// </summary>
    [JsonPropertyName(nameof(User))]
    User,

    /// <summary>
    /// The entity is owned by an organization.
    /// </summary>
    [JsonPropertyName(nameof(Organization))]
    Organization,
}
