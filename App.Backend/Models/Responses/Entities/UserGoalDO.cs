// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

/// <summary>
/// Data object representing a user's goal enrollment.
/// </summary>
public class UserGoalDO : BaseEntityDO<UserGoal>
{
    public UserGoalDO() { }

    public UserGoalDO(UserGoal userGoal) : base(userGoal)
    {
        UserId = userGoal.UserId;
        GoalId = userGoal.GoalId;
        State = userGoal.State;
        UnlocksAt = userGoal.UnlocksAt;
        Goal = userGoal.Goal;
        User = userGoal.User;
    }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid GoalId { get; set; }

    [Required]
    public EntityObjectState State { get; set; }

    [Required]
    public DateTimeOffset? UnlocksAt { get; set; }

    /// <summary>
    /// The goal the user is enrolled in.
    /// </summary>
    [Required]
    public GoalDO Goal { get; set; } = null!;

    /// <summary>
    /// The user enrolled in this goal.
    /// </summary>
    [Required]
    public UserLightDO User { get; set; } = null!;

    public static implicit operator UserGoalDO?(UserGoal? userGoal) =>
        userGoal is null ? null : new(userGoal);
}
