// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.Text.Json.Serialization;

namespace Backend.API.Domain.Enums;

/// <summary>
/// The different kinds of feeds that exist.
/// </summary>
[JsonConverter(typeof(JsonStringEnumMemberNameAttribute))]
public enum SearchKind
{
    /// <summary>
    /// A new user has joined the app
    /// </summary>
    [JsonPropertyName(nameof(User))]
    User = 1 << 0,

    /// <summary>
    /// Some user has completed a project, hoorah!
    /// </summary>
    [JsonPropertyName(nameof(Project))]
    Project = 1 << 1,
}
