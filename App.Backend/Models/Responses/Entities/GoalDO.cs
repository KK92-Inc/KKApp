// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

/// <summary>
/// Data object representing a goal/learning objective.
/// </summary>
public class GoalDO : BaseEntityDO<Goal>
{
    public GoalDO() { }

    public GoalDO(Goal goal) : base(goal)
    {
        Name = goal.Name;
        Description = goal.Description;
        Slug = goal.Slug;
        Thumbnail = goal.Thumbnail;
        Enabled = goal.Enabled;
        Public = goal.Public;
        Deprecated = goal.Deprecated;
        Workspace = goal.Workspace;
    }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    [Required]
    public string Slug { get; set; } = null!;

    [Required, Description("The URL to the image containing the thumbnail of the goal.")]
    public string? Thumbnail { get; set; }

    [Required]
    public bool Enabled { get; set; }

    [Required]
    public bool Public { get; set; }

    [Required]
    public bool Deprecated { get; set; }

    [Required]
    public WorkspaceDO Workspace { get; set; } = null!;

    public static implicit operator GoalDO?(Goal? goal) => goal is null ? null : new(goal);
}
