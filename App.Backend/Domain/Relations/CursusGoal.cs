// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Domain.Relations;

/// <summary>
/// Represents a goal node within a cursus track.
///
/// Uses an adjacency list pattern to form a tree/DAG:
/// - Root nodes have <see cref="ParentGoalId"/> set to null.
/// - Child nodes reference another goal in the same cursus via <see cref="ParentGoalId"/>.
/// - <see cref="Position"/> determines ordering among siblings at the same level.
/// </summary>
[Table("rel_cursus_goal")]
[PrimaryKey(nameof(CursusId), nameof(GoalId))]
[Index(nameof(CursusId), nameof(ParentGoalId))]
public class CursusGoal : BaseTimestampEntity
{
    [Column("cursus_id")]
    public Guid CursusId { get; set; }

    [Column("goal_id")]
    public Guid GoalId { get; set; }

    /// <summary>
    /// The position/order of this goal among its siblings.
    /// Goals with the same parent are ordered by this value.
    /// </summary>
    [Column("position")]
    public int Position { get; set; }

    /// <summary>
    /// References another GoalId within the same cursus to form the hierarchy.
    /// Null for root-level goals.
    /// </summary>
    [Column("parent_goal_id")]
    public Guid? ParentGoalId { get; set; }

    [ForeignKey(nameof(CursusId))]
    public virtual Cursus Cursus { get; set; }

    [ForeignKey(nameof(GoalId))]
    public virtual Goal Goal { get; set; }
}
