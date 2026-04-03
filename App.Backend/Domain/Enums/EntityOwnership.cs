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
    [JsonPropertyName(nameof(User))]
    User,

    [JsonPropertyName(nameof(Organization))]
    Organization,
}
