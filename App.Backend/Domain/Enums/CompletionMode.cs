// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Enums;

/// <summary>
/// Determines how a user progresses through a cursus track.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CompletionMode
{
    /// <summary>
    /// Level-ordered progression: the user must complete all goals at depth N
    /// before goals at depth N+1 become available. Think of concentric rings
    /// expanding outward from the root.
    /// </summary>
    [JsonPropertyName(nameof(Ring))]
    Ring,

    /// <summary>
    /// Free-roam progression: each root-to-leaf branch is independent.
    /// A user can progress down any branch without completing sibling branches.
    /// The only prerequisite is that a goal's direct parent is completed.
    /// </summary>
    [JsonPropertyName(nameof(FreeStyle))]
    FreeStyle,
}
