// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;
using App.Backend.Domain.Enums;

namespace App.Backend.Domain.Rules;

/// <summary>
/// Base class for eligibility rules.
/// Rules are used to determine if a user is eligible to perform or receive a review.
/// These are stored as JSON in the database.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(MinReviewsCompletedRule), nameof(RuleType.MinReviewsCompleted))]
[JsonDerivedType(typeof(HasProjectRule), nameof(RuleType.HasCompletedProject))]
[JsonDerivedType(typeof(MinProjectsCompletedRule), nameof(RuleType.MinProjectsCompleted))]
[JsonDerivedType(typeof(MinDaysRegisteredRule), nameof(RuleType.MinDaysRegistered))]
[JsonDerivedType(typeof(SameTimezoneRule), nameof(RuleType.SameTimezone))]
[JsonDerivedType(typeof(IsMemberRule), nameof(RuleType.IsTeamMember))]
[JsonDerivedType(typeof(HasCursusRule), nameof(RuleType.EnrolledInCursus))]
// [JsonDerivedType(typeof(HasCursusRoleRule), nameof(RuleType.HasCursusRole))]
[JsonDerivedType(typeof(AllOfRule), nameof(RuleType.AllOf))]
[JsonDerivedType(typeof(AnyOfRule), nameof(RuleType.AnyOf))]
[JsonDerivedType(typeof(NotRule), nameof(RuleType.Not))]
public abstract class Rule
{
    /// <summary>
    /// A human-readable description of what this rule checks.
    /// Used for display in the frontend.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
