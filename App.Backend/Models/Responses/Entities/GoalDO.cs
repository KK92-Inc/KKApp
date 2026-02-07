// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

/// <summary>
/// Data object representing a goal/learning objective.
/// </summary>
public class GoalDO(Goal goal) : BaseEntityDO<Goal>(goal)
{
    [Required]
    public string Name { get; set; } = goal.Name;

    [Required]
    public string Description { get; set; } = goal.Description;

    [Required]
    public string Slug { get; set; } = goal.Slug;

    public static implicit operator GoalDO?(Goal? goal) => goal is null ? null : new(goal);
}
