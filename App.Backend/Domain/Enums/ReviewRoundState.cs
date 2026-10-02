// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Enums;

/// <summary>
/// The lifecycle of a single evaluation attempt (round) of a user project.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReviewRoundState
{
    /// <summary>
    /// The round is collecting reviews. A user project has at most one open round.
    /// </summary>
    [JsonPropertyName(nameof(Open))]
    Open = 0,

    /// <summary>
    /// Every required review was finished and every verdict was a pass.
    /// </summary>
    [JsonPropertyName(nameof(Passed))]
    Passed = 1,

    /// <summary>
    /// At least one verdict was a fail. The team has to request a new round.
    /// </summary>
    [JsonPropertyName(nameof(Failed))]
    Failed = 2,

    /// <summary>
    /// The round was abandoned before it resolved (e.g. the leader cancelled it).
    /// </summary>
    [JsonPropertyName(nameof(Cancelled))]
    Cancelled = 3
}
