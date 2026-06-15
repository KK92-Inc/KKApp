// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Domain.Values.Misc;

// ============================================================================

/// <summary>
/// Represents a range of text in a document, defined by a start and end position.
/// </summary>
/// <param name="Start">The start line and column.</param>
/// <param name="End">The end line and column.</param>
public record TextRange(int StartLine, int StartColumn, int EndLine, int EndColumn);