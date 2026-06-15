// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

// ============================================================================

using App.Backend.Domain.Values.Misc;

namespace App.Backend.Domain.Values;

/// <summary>
/// Represents a text annotation data that contains a comment string.
/// </summary>
/// <param name="Comment">The comment associated with the annotation.</param>
/// <param name="Range">The range of text to be annotated.</param>
public sealed record TextAnnotationData(string Comment, TextRange Range) : AnnotationData;
