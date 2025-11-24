// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.Text.Json.Serialization;

namespace Backend.API.Domain.Enums;

/// <summary>
/// The different states that an entity can have.
///
/// For example the state of a project
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskState
{
    /// <summary>
    /// The task is inactive and won't continue.
    /// </summary>
    [JsonPropertyName(nameof(Inactive))]
    Inactive,

    /// <summary>
    /// The task is active and will continue.
    /// </summary>
    [JsonPropertyName(nameof(Active))]
    Active,

    /// <summary>
    /// The task is awaiting something.
    /// </summary>
    [JsonPropertyName(nameof(Awaiting))]
    Awaiting,

    /// <summary>
    /// The entity is completed.
    /// </summary>
    [JsonPropertyName(nameof(Completed))]
    Completed
}
