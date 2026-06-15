// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Values.Misc;

// ============================================================================

namespace App.Backend.Domain.Values;

/// <summary>
/// Represents a drawing annotation data that contains a color and a list of points.
/// </summary>
/// <param name="Color">The color of the drawing.</param>
/// <param name="Points">The list of points that define the drawing.</param>
public sealed record DrawingAnnotationData(
    string Color,
    IReadOnlyList<Point2D> Points
) : AnnotationData;
