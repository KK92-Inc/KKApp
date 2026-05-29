// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Domain.Entities.Users;

[Table("tbl_user_goal")]
public class UserGoal : BaseEntity
{
    public UserGoal()
    {
        UserId = Guid.Empty;
        User = null!;

        GoalId = Guid.Empty;
        Goal = null!;

        State = EntityObjectState.Active;
        UnlocksAt = null;
    }

    [Column("state")]
    public EntityObjectState State { get; set; }

    /// <summary>
    /// If set, locks modifications on the entity until the specified time.
    /// E.g: Used for cooldowns after unsubscribing.
    /// </summary>
    [Column("unlocks_at")]
    public DateTimeOffset? UnlocksAt { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }

    [Column("goal_id")]
    public Guid GoalId { get; set; }

    [ForeignKey(nameof(GoalId))]
    public virtual Goal Goal { get; set; }
}
