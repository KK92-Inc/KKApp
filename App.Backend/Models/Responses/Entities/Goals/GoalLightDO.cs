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
public class GoalLightDO : BaseEntityDO<Goal>
{
    public GoalLightDO() { }

    public GoalLightDO(Goal goal) : base(goal)
    {
        Name = goal.Name;
        Slug = goal.Slug;
        Enabled = goal.Enabled;
        Public = goal.Public;
        Deprecated = goal.Deprecated;
    }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Slug { get; set; } = null!;

    [Required]
    public bool Enabled { get; set; }

    [Required]
    public bool Public { get; set; }

    [Required]
    public bool Deprecated { get; set; }

    public static implicit operator GoalLightDO?(Goal? goal) =>
        goal is null ? null : new(goal);
}
