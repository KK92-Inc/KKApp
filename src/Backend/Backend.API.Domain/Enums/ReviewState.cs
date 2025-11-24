// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.Text.Json.Serialization;

namespace Backend.API.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReviewState
{
    /// <summary>
    /// The review is pending, waiting for a reviewer to start.
    /// </summary>
    [JsonPropertyName(nameof(Pending))]
    Pending,

    /// <summary>
    /// The review is in progress, the reviewer is reviewing the entity.
    /// </summary>
    [JsonPropertyName(nameof(InProgress))]
    InProgress,

    /// <summary>
    /// The review is finished, the reviewer has finished reviewing the entity.
    /// </summary>
    [JsonPropertyName(nameof(Finished))]
    Finished
}
