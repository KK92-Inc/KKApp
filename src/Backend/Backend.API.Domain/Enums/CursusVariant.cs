// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace Backend.API.Domain.Enums;

/// <summary>
/// The different kinds of cursi that exist.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CursusVariant
{
    /// <summary>
    /// A dynamic cursus is one whose content is generated on the fly.
    ///
    /// For instance if the user is learning a new programming language, the content of the cursus
    /// can be generated based on the user's choice.
    ///
    /// Perhaps it starts of with a project about creating a simple calculator, and then the user
    /// can choose to learn about creating a simple game next.
    /// </summary>
    [JsonPropertyName(nameof(Dynamic))]
    Dynamic,

    /// <summary>
    /// A fixed traditional path for a cursus with set in goals that need to be achieved.
    /// </summary>
    [JsonPropertyName(nameof(Fixed))]
    Fixed,
}
