// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;
using App.Backend.Models.Responses.Entities.Projects;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Reviews;

/// <summary>
/// Data object representing a review.
/// </summary>
public class ReviewDO(Review review) : BaseEntityDO<Review>(review)
{
    [Required]
    public ReviewKinds Kind { get; set; } = review.Kind;

    [Required]
    public ReviewState State { get; set; } = review.State;

    /// <summary>
    /// The evaluation round this review is a slot of. Null means it is advisory
    /// feedback that doesn't count towards completing the project.
    /// </summary>
    [Required]
    public Guid? RoundId { get; set; } = review.RoundId;

    /// <summary>
    /// The reviewer's verdict ("is this project a pass?"), once the review is finished.
    /// </summary>
    [Required]
    public bool? Passed { get; set; } = review.Passed;

    [Required]
    public DateTimeOffset? FinishedAt { get; set; } = review.FinishedAt;

    [Required]
    public UserProjectLightDO UserProject { get; set; } = review.UserProject;

    /// <summary>
    /// The user performing the review, if assigned.
    /// </summary>
    [Required]
    public UserLightDO? Reviewer { get; set; } = review.Reviewer;

    /// <summary>
    /// The rubric used for this review.
    /// </summary>
    [Required]
    public RubricLightDO Rubric { get; set; } = review.Rubric;

    public static implicit operator ReviewDO?(Review? review) =>
        review is null ? null : new(review);
}
