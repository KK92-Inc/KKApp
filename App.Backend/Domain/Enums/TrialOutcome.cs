// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Enums;

/// <summary>
/// The verdict on a user after their trial: were they selected to become a student?
/// Decided by staff, never by the system.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TrialOutcome
{
    /// <summary>
    /// No verdict yet.
    /// </summary>
    [JsonPropertyName(nameof(Pending))]
    Pending = 0,

    /// <summary>
    /// Selected, the user may move on to pick a kickoff.
    /// </summary>
    [JsonPropertyName(nameof(Selected))]
    Selected = 1,

    /// <summary>
    /// Not selected, the user will not join.
    /// </summary>
    [JsonPropertyName(nameof(Rejected))]
    Rejected = 2,
}
