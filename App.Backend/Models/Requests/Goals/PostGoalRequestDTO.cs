// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Goals;

// ============================================================================

/// <summary>
/// Request DTO for creating a new goal.
/// </summary>
public record PostGoalRequestDTO
{
    /// <summary>
    /// The name of the goal.
    /// </summary>
    [Required, StringLength(256, MinimumLength = 1)]
    public required string Name { get; init; }

    // /// <summary>
    // /// The unique slug for the goal.
    // /// </summary>
    // [Required, StringLength(256, MinimumLength = 1)]
    // [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$",
    //     ErrorMessage = "Slug must be lowercase alphanumeric with hyphens only")]
    // public required string Slug { get; init; }

    /// <summary>
    /// Optional description of the goal.
    /// </summary>
    [Required, StringLength(16384)]
    [Description("Optional description of the goal.")]
    public required string Description { get; init; }

    /// <summary>
    /// Whether the goal is active.
    /// </summary>
    [Description("Indicates whether the goal is currently active.")]
    public bool Active { get; init; } = true;

    /// <summary>
    /// Whether the project is public.
    /// </summary>
    [Description("Indicates whether the goal is publicly visible.")]
    public bool Public { get; init; } = false;
}
