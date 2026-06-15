// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Enums;

/// <summary>
/// Represents the kind of annotation.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AnnotationKind
{
    /// <summary>
    /// Represents a text annotation, which typically contains a comment or note.
    /// </summary>
    [JsonPropertyName(nameof(Text))]
    Text = 0,

    /// <summary>
    /// Represents a suggestion annotation, which may contain proposed changes or recommendations.
    /// </summary>
    [JsonPropertyName(nameof(Suggestion))]
    Suggestion = 1,

    /// <summary>
    /// Represents a drawing annotation, which may contain visual elements or sketches.
    /// </summary>
    [JsonPropertyName(nameof(Drawing))]
    Drawing = 2,
}
