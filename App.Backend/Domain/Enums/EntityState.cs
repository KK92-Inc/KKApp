// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Enums;

/// <summary>
/// General purpose entity states.
/// E.g: User Project is Active, Inactive, Awaiting or Completed.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EntityObjectState
{
    /// <summary>
    /// The entity is inactive and won't continue.
    /// </summary>
    [JsonPropertyName(nameof(Inactive))]
    Inactive,

    [JsonPropertyName(nameof(Active))]
    Active,

    [JsonPropertyName(nameof(Awaiting))]
    Awaiting,

    [JsonPropertyName(nameof(Completed))]
    Completed
}
