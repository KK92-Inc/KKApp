// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;
using App.Backend.Domain.Enums;

namespace App.Backend.Domain.Values;

/// <summary>
/// Represents a text annotation data that contains a comment string.
/// </summary>
public sealed record CommentAnnotationData(string Filepath, string Body, int RowStart, int RowEnd) : AnnotationData
{
    [JsonIgnore]
    public override AnnotationKind Kind => AnnotationKind.Comment;
}
