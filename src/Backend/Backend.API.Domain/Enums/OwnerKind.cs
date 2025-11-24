// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.Text.Json.Serialization;

namespace Backend.API.Domain.Enums;

/// <summary>
/// Something being either owned by a user or an Org.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OwnerKind
{
    /// <summary>
    /// External source control other than Github. E.g: (Gitea, Gitlab, Custom Home server, ...)
    /// </summary>
    [JsonPropertyName(nameof(User))]
    User,

    /// <summary>
    /// Uses the backend configured git source control, the ideal *boomer* option as most people just
    /// want to "upload a file" but we want to provide source control towards whatever they provide.
    /// </summary>
    [JsonPropertyName(nameof(Organization))]
    Organization,
}
