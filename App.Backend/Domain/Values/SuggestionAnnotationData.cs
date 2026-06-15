// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

// ============================================================================

using App.Backend.Domain.Values.Misc;

namespace App.Backend.Domain.Values;

/// <summary>
/// Represents a suggestion annotation data that contains a replacement string.
/// This record is used to store information about a suggestion made in the context of an annotation.
/// </summary>
/// <param name="Replacement">The string to replace the annotated text with.</param>
/// <param name="TextRange">The range of text to be replaced.</param>
public sealed record SuggestionAnnotationData(string Replacement, TextRange TextRange) : AnnotationData;
