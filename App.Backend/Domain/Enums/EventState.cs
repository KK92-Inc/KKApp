// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventState
{
    /// <summary>
    /// The Event is pending further action, e.g: Enough users joining.
    /// </summary>
    [JsonPropertyName(nameof(Pending))]
    Pending = 0,

    /// <summary>
    /// The event is accepted and public, waiting for it to start.
    /// </summary>
    [JsonPropertyName(nameof(Accepted))]
    Accepted = 1,

    /// <summary>
    /// Event was rejected and will not happen.
    /// </summary>
    [JsonPropertyName(nameof(Rejected))]
    Rejected = 2,

    /// <summary>
    /// The event has concluded.
    /// </summary>
    [JsonPropertyName(nameof(Finished))]
    Finished = 3,
}

