// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
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
    }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }

    [Column("goal_id")]
    public Guid GoalId { get; set; }

    [ForeignKey(nameof(GoalId))]
    public virtual Goal Goal { get; set; }

    [Column("state")]
    public EntityObjectState State { get; set; }
}
