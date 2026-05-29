// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Domain.Relations;

/// <summary>
/// A frozen snapshot of a cursus track at the moment a user enrolled.
///
/// Mirrors <see cref="CursusGoal"/> but is keyed to a <see cref="UserCursus"/>
/// rather than a <see cref="Cursus"/>. This means track restructures after
/// enrollment never create holes in a user's history — their personal track
/// is immutable from the point of subscription onward.
/// </summary>
[Table("rel_cursus_goal_snapshot")]
[PrimaryKey(nameof(UserCursusId), nameof(GoalId))]
[Index(nameof(UserCursusId), nameof(ParentGoalId))]
[Index(nameof(UserCursusId), nameof(ChoiceGroup), nameof(GoalId))]
public class UserCursusGoal : BaseTimestampEntity
{
    [Column("user_cursus_id")]
    public Guid UserCursusId { get; set; }

    [Column("goal_id")]
    public Guid GoalId { get; set; }

    /// <summary>
    /// Mirrors <see cref="CursusGoal.ParentGoalId"/> at snapshot time.
    /// Null for root-level goals.
    /// </summary>
    [Column("parent_goal_id")]
    public Guid? ParentGoalId { get; set; }

    /// <summary>
    /// Mirrors <see cref="CursusGoal.ChoiceGroup"/> at snapshot time.
    /// Null means the goal is required (not part of any choice set).
    /// </summary>
    [Column("choice_group")]
    public Guid? ChoiceGroup { get; set; }

    [ForeignKey(nameof(UserCursusId))]
    public virtual UserCursus UserCursus { get; set; } = null!;

    [ForeignKey(nameof(GoalId))]
    public virtual Goal Goal { get; set; } = null!;
}