// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace Backend.API.Domain.Enums;

/// <summary>
/// Enum flags to track notification preferences of a user.
/// </summary>
[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationPreference
{
    /// <summary>
    /// No notifications
    /// </summary>
    [JsonPropertyName(nameof(None))]
    None = 0,

    /// <summary>
    /// Preference for Invite notifications
    /// </summary>
    [JsonPropertyName(nameof(Invite))]
    Invite = 1 << 0,

    /// <summary>
    /// Preference for Review notifications
    /// </summary>
    [JsonPropertyName(nameof(Review))]
    Review = 1 << 1,

    /// <summary>
    /// Preference for General notifications
    /// </summary>
    [JsonPropertyName(nameof(General))]
    General = 1 << 2,

    /// <summary>
    /// Preference for Event / Spotlight notifications
    /// </summary>
    [JsonPropertyName(nameof(Event))]
    Event = 1 << 3,

    /// <summary>
    /// All notification preferences enabled
    /// </summary>
    [JsonPropertyName(nameof(All))]
    All = Invite | Review | General | Event
}
