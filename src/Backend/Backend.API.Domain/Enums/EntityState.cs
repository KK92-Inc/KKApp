// ============================================================================
// Copyright (c) 2025 - W2Inc.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace Backend.API.Domain.Enums;

/// <summary>
/// General purpose entity states.
/// E.g: User Project is Active, Inactive, Awaiting or Completed.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EntityState
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
