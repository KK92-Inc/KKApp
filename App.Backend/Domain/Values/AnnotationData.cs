// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Domain.Values;

/// <summary>
/// Represents the base class for different types of annotation data.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(CommentAnnotationData), nameof(AnnotationKind.Comment))]
[JsonDerivedType(typeof(ConclusionAnnotationData), nameof(AnnotationKind.Conclusion))]
public abstract record AnnotationData
{
    [JsonIgnore]
    public abstract AnnotationKind Kind { get; }
}
