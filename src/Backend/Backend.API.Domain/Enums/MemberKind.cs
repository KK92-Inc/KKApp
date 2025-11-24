// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace Backend.API.Domain.Enums;

/// <summary>
/// Specifies the kind of member within a group or organization.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MemberKind
{
    /// <summary>
    /// Represents a member who holds a leadership position.
    /// </summary>
    [JsonPropertyName(nameof(Leader))]
    Leader,

    /// <summary>
    /// Represents a member who holds a assisting position.
    /// </summary>
    [JsonPropertyName(nameof(Pawn))]
    Pawn,
}
