// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Enums;

/// <summary>
/// Certain kinds of ownership for an entity.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EntityOwnership
{
    [JsonPropertyName(nameof(User))]
    User,

    [JsonPropertyName(nameof(Organization))]
    Organization,
}
