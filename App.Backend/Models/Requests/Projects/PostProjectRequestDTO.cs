// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
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
    [Description("The name of the project.")]
    public required string Name { get; init; }

    /// <summary>
    /// Optional description of the project.
    /// </summary>
    [Required, StringLength(2048, MinimumLength = 1)]
    [Description("Optional description of the project.")]
    public required string Description { get; init; }

    /// <summary>
    /// Whether the project is active.
    /// </summary>
    [Description("Indicates whether the project is currently active.")]
    public bool Active { get; init; } = true;

    /// <summary>
    /// Whether the project is public.
    /// </summary>
    [Description("Indicates whether the project is currently public.")]
    public bool Public { get; init; } = false;

    /// <summary>
    /// The maximum number of members allowed in the project.
    /// </summary>
    [Range(1, 10)]
    [Description("The maximum number of members allowed in the project.")]
    public int MaxMembers { get; init; } = 1;
}
