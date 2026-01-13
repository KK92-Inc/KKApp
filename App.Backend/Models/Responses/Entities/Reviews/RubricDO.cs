// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Rules;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Reviews;

/// <summary>
/// Data object representing a rubric for reviews.
/// </summary>
public class RubricDO(Rubric rubric) : BaseEntityDO<Rubric>(rubric)
{
    [Required]
    public string Name { get; set; } = rubric.Name;

    [Required]
    public string Slug { get; set; } = rubric.Slug;

    [Required]
    public string Markdown { get; set; } = rubric.Markdown;

    [Required]
    public bool Public { get; set; } = rubric.Public;

    [Required]
    public bool Enabled { get; set; } = rubric.Enabled;

    [Required]
    public ReviewKinds SupportedReviewKinds { get; set; } = rubric.SupportedReviewKinds;

    [Required]
    public Guid CreatorId { get; set; } = rubric.CreatorId;

    public Guid? GitInfoId { get; set; } = rubric.GitInfoId;

    /// <summary>
    /// The user who created this rubric.
    /// </summary>
    public UserLightDO? Creator { get; set; } = rubric.Creator;

    /// <summary>
    /// Git information for this rubric.
    /// </summary>
    public GitDO? GitInfo { get; set; } = rubric.GitInfo;

    /// <summary>
    /// Rules that determine who can be a reviewer.
    /// </summary>
    // public List<Rule> ReviewerEligibilityRules { get; set; } = rubric.ReviewerEligibilityRules;

    /// <summary>
    /// Rules that determine who can request a review.
    /// </summary>
    // public List<Rule> RevieweeEligibilityRules { get; set; } = rubric.RevieweeEligibilityRules;

    public static implicit operator RubricDO?(Rubric? rubric) =>
        rubric is null ? null : new(rubric);
}
