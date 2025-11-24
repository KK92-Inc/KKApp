// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace Backend.API.Domain.Enums;

/// <summary>
/// The type of action the user undertook for a given notification
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationState
{

    [JsonPropertyName(nameof(Unprocessed))]
    Unprocessed,


    [JsonPropertyName(nameof(Dispatched))]
    Dispatched,


    [JsonPropertyName(nameof(Failed))]
    Failed,
}
