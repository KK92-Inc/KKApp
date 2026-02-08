// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;
using App.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Domain.Relations;

/// <summary>
/// Relation between Goal and Project entities.
/// E.g: A Goal can have multiple Projects associated with it.
/// </summary>
[Table("rel_goal_project")]
[PrimaryKey(nameof(ProjectId), nameof(GoalId))]
public class GoalProject : BaseTimestampEntity
{
    [Column("goal_id", Order = 0)]
    public Guid GoalId { get; set; }

    [Column("project_id", Order = 1)]
    public Guid ProjectId { get; set; }

    public virtual Project Project { get; set; }
    public virtual Goal Goal { get; set; }
}
