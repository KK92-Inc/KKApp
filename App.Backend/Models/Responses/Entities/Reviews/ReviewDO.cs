// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Reviews;

public class ReviewDO : BaseEntityDO<Review>
{
    public ReviewDO() { }

    public ReviewDO(Review review) : base(review)
    {
        Kind = review.Kind;
        State = review.State;
        RoundId = review.RoundId;
        Ref = review.Ref;
        Sha = review.Sha;
        Passed = review.Passed;
        ClaimedAt = review.ClaimedAt;
        StartedAt = review.StartedAt;
        FinishedAt = review.FinishedAt;
        UserProject = new(review.UserProject);
        Reviewer = review.Reviewer;
        Rubric = new(review.Rubric);
    }

    [Required]
    public ReviewKinds Kind { get; set; }

    [Required]
    public ReviewState State { get; set; }

    /// <summary>
    /// The evaluation round this review is a slot of. Null means it is advisory
    /// feedback that doesn't count towards completing the project.
    /// </summary>
    [Required]
    public Guid? RoundId { get; set; }

    /// <summary>
    /// The branch that was targeted when the review was created. It moves with every push, use <see cref="Sha"/> for what was reviewed.
    /// </summary>
    [Required]
    public string Ref { get; set; } = null!;

    /// <summary>
    /// The exact commit that was reviewed. Null for reviews created before SHAs were recorded.
    /// </summary>
    [Required]
    public string? Sha { get; set; }

    /// <summary>
    /// The reviewer's verdict ("is this project a pass?"), once the review is finished.
    /// </summary>
    [Required]
    public bool? Passed { get; set; }

    /// <summary>
    /// When the reviewer claimed this review. They then have 2 days to start it, and 24 hours to finish it once started.
    /// </summary>
    [Required]
    public DateTimeOffset? ClaimedAt { get; set; }

    /// <summary>
    /// When the reviewer started. A started review has 24 hours to be finished before it is released.
    /// </summary>
    [Required]
    public DateTimeOffset? StartedAt { get; set; }

    [Required]
    public DateTimeOffset? FinishedAt { get; set; }

    [Required]
    public ReviewProjectDO UserProject { get; set; } = null!;

    /// <summary>
    /// The user performing the review, if assigned.
    /// </summary>
    [Required]
    public UserBriefDO? Reviewer { get; set; }

    [Required]
    public ReviewRubricDO Rubric { get; set; } = null!;

    public static implicit operator ReviewDO?(Review? review) =>
        review is null ? null : new(review);
}
