// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;
using App.Backend.Domain.Rules.Evaluations;
using App.Backend.Domain.Rules.Evaluations.Composites;

// ============================================================================

namespace App.Backend.Domain;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(AllOfRule), "all_of")]
[JsonDerivedType(typeof(AnyOfRule), "any_of")]
[JsonDerivedType(typeof(NotRule), "not")]
[JsonDerivedType(typeof(HasCursusRule), "has_cursus")]
[JsonDerivedType(typeof(HasProjectRule), "has_project")]
[JsonDerivedType(typeof(IsMemberRule), "is_member")]
[JsonDerivedType(typeof(MinDaysRegisteredRule), "min_days_registered")]
[JsonDerivedType(typeof(MinProjectsCompletedRule), "min_projects_completed")]
[JsonDerivedType(typeof(MinReviewsCompletedRule), "min_reviews_completed")]
[JsonDerivedType(typeof(SameTimezoneRule), "same_timezone")]
public abstract record Rule
{
    /// <summary>
    /// Overrides the default failure message shown to the user.
    /// </summary>
    public string? Description { get; init; }
}
