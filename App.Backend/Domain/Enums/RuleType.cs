// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

namespace App.Backend.Domain.Enums;

/// <summary>
/// Types of eligibility rules that can be used to determine if a user
/// can perform or receive a review.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RuleType
{
    /// <summary>
    /// User must have completed at least X reviews.
    /// </summary>
    [JsonPropertyName(nameof(MinReviewsCompleted))]
    MinReviewsCompleted,

    /// <summary>
    /// User must have completed a specific project (by slug).
    /// </summary>
    [JsonPropertyName(nameof(HasCompletedProject))]
    HasCompletedProject,

    /// <summary>
    /// User must have completed at least X projects.
    /// </summary>
    [JsonPropertyName(nameof(MinProjectsCompleted))]
    MinProjectsCompleted,

    /// <summary>
    /// User must be registered for at least X days.
    /// </summary>
    [JsonPropertyName(nameof(MinDaysRegistered))]
    MinDaysRegistered,

    /// <summary>
    /// User must be in the same timezone as the reviewee.
    /// Useful for peer reviews.
    /// </summary>
    [JsonPropertyName(nameof(SameTimezone))]
    SameTimezone,

    /// <summary>
    /// User must be a member of the same team/project group.
    /// Useful for self-evaluation reviews.
    /// </summary>
    [JsonPropertyName(nameof(IsTeamMember))]
    IsTeamMember,

    /// <summary>
    /// User must be enrolled in a specific cursus.
    /// </summary>
    [JsonPropertyName(nameof(EnrolledInCursus))]
    EnrolledInCursus,

    /// <summary>
    /// User must have a specific role in a cursus.
    /// </summary>
    [JsonPropertyName(nameof(HasCursusRole))]
    HasCursusRole,

    /// <summary>
    /// Logical AND: All nested rules must be satisfied.
    /// </summary>
    [JsonPropertyName(nameof(AllOf))]
    AllOf,

    /// <summary>
    /// Logical OR: At least one nested rule must be satisfied.
    /// </summary>
    [JsonPropertyName(nameof(AnyOf))]
    AnyOf,

    /// <summary>
    /// Logical NOT: The nested rule must NOT be satisfied.
    /// </summary>
    [JsonPropertyName(nameof(Not))]
    Not
}
