// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Users;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

/// <summary>
/// Data object representing a user's goal enrollment.
/// </summary>
public class UserGoalDO(UserGoal userGoal) : BaseEntityDO<UserGoal>(userGoal)
{
    [Required]
    public Guid UserId { get; set; } = userGoal.UserId;

    [Required]
    public Guid GoalId { get; set; } = userGoal.GoalId;

    /// <summary>
    /// The goal the user is enrolled in.
    /// </summary>
    public GoalDO? Goal { get; set; } = userGoal.Goal;

    /// <summary>
    /// The user enrolled in this goal.
    /// </summary>
    public UserLightDO? User { get; set; } = userGoal.User;

    public static implicit operator UserGoalDO?(UserGoal? userGoal) =>
        userGoal is null ? null : new(userGoal);
}
