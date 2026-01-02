// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Users;

namespace App.Backend.Core.Rules;

/// <summary>
/// Context for evaluating eligibility rules.
/// Contains all the information needed to evaluate rules.
/// </summary>
public record RuleContext
{
    /// <summary>
    /// The user being evaluated (reviewer or reviewee).
    /// </summary>
    public required User User { get; init; }

    /// <summary>
    /// The user project being reviewed (if applicable).
    /// </summary>
    public UserProject? UserProject { get; init; }

    /// <summary>
    /// The current date/time for time-based rules.
    /// </summary>
    public DateTimeOffset Now { get; init; } = DateTimeOffset.UtcNow;
}
