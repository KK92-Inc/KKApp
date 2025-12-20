// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Cursus;

// ============================================================================

/// <summary>
/// Request DTO for updating a cursus (partial update).
/// </summary>
public record PatchCursusRequestDTO
{
    /// <summary>
    /// Optional name update.
    /// </summary>
    [StringLength(256, MinimumLength = 1)]
    public string? Name { get; init; }

    /// <summary>
    /// Optional slug update.
    /// </summary>
    [StringLength(256, MinimumLength = 1)]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$",
        ErrorMessage = "Slug must be lowercase alphanumeric with hyphens only")]
    public string? Slug { get; init; }

    /// <summary>
    /// Optional description update.
    /// </summary>
    [StringLength(16384)]
    public string? Description { get; init; }

    /// <summary>
    /// Optional active status update.
    /// </summary>
    public bool? Active { get; init; }

    /// <summary>
    /// Optional deprecated status update.
    /// </summary>
    public bool? Deprecated { get; init; }

    /// <summary>
    /// Optional track definition update as JSON.
    /// </summary>
    public string? Track { get; init; }
}
