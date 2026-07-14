// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Goals;

// ============================================================================

/// <summary>
/// Request DTO for updating a goal (partial update).
/// </summary>
public record PatchGoalRequestDTO
{
    /// <summary>
    /// Optional name update.
    /// </summary>
    [StringLength(256, MinimumLength = 2)]
    [Description("The name of the goal.")]
    public string? Name { get; init; }

    /// <summary>
    /// Optional description update.
    /// </summary>
    [StringLength(2000)]
    [Description("Optional description of the goal.")]
    public string? Description { get; init; }

    /// <summary>
    /// Optional active status update.
    /// </summary>
    [Description("Indicates whether the goal is currently active.")]
    public bool? Active { get; init; }

    /// <summary>
    /// Optional active status update.
    /// </summary>
    [Description("Indicates whether the goal is publicly visible.")]
    public bool? Public { get; init; }

    [Required, MinLength(1), MaxLength(4)]
    [Description("The list of project IDs to initalize this goal with.")]
    public required IEnumerable<Guid> Projects { get; init; }
}
