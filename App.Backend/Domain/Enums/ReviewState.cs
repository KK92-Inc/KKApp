// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReviewState
{
    /// <summary>
    /// The review is pending, waiting for a reviewer to start.
    /// </summary>
    [JsonPropertyName(nameof(Pending))]
    Pending = 0,

    /// <summary>
    /// The review is in progress, the reviewer is reviewing the entity.
    /// </summary>
    [JsonPropertyName(nameof(InProgress))]
    InProgress = 1,

    /// <summary>
    /// The review is finished, the reviewer has finished reviewing the entity.
    /// </summary>
    [JsonPropertyName(nameof(Finished))]
    Finished = 2,

    /// <summary>
    /// The review is cancelled, the reviewer has cancelled the review.
    /// </summary>
    [JsonPropertyName(nameof(Cancelled))]
    Cancelled = 3
}
