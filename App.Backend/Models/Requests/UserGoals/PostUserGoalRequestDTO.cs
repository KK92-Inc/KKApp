// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.UserGoals;

// ============================================================================

/// <summary>
/// Request DTO for subscribing to a goal.
/// </summary>
public record PostUserGoalRequestDTO
{
    /// <summary>
    /// The goal ID to subscribe to.
    /// </summary>
    [Required]
    public required Guid GoalId { get; init; }
}
