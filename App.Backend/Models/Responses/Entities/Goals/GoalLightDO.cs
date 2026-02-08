// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Goals;

/// <summary>
/// A lightweight version of <see cref="Goal"/> containing only minimal information.
/// Used in nested responses (e.g., track nodes) to avoid heavy payloads.
/// </summary>
public class GoalLightDO(Goal goal) : BaseEntityDO<Goal>(goal)
{
    [Required]
    public string Name { get; set; } = goal.Name;

    [Required]
    public string Slug { get; set; } = goal.Slug;

    [Required]
    public bool Active { get; set; } = goal.Active;

    [Required]
    public bool Deprecated { get; set; } = goal.Deprecated;

    public static implicit operator GoalLightDO?(Goal? goal) =>
        goal is null ? null : new(goal);
}
