// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Projects;

// ============================================================================

/// <summary>
/// Request DTO for creating a new project.
/// </summary>
public record PostProjectRequestDTO
{
    /// <summary>
    /// The name of the project.
    /// </summary>
    [Required, StringLength(256, MinimumLength = 1)]
    public required string Name { get; init; }

    // /// <summary>
    // /// The unique slug for the project.
    // /// </summary>
    // [Required, StringLength(256, MinimumLength = 1)]
    // [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$",
    //     ErrorMessage = "Slug must be lowercase alphanumeric with hyphens only")]
    // public required string Slug { get; init; }

    /// <summary>
    /// Optional description of the project.
    /// </summary>
    [Required, StringLength(16384)]
    public string? Description { get; init; }

    /// <summary>
    /// Whether the project is active.
    /// </summary>
    public bool Active { get; init; } = true;

    /// <summary>
    /// Whether the project is public.
    /// </summary>
    public bool Public { get; init; } = false;
}
