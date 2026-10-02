// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;
using App.Backend.Domain.Enums;

namespace App.Backend.Domain.Values;

/// <summary>
/// Represents a text annotation data that concludes a evaluation.
/// E.g: "Overall this project is pretty solid, could use some more time..."
/// </summary>
/// <param name="Comment">The comment associated with the annotation.</param>
public sealed record ConclusionAnnotationData(string Body) : AnnotationData
{
    [JsonIgnore]
    public override AnnotationKind Kind => AnnotationKind.Conclusion;
}
