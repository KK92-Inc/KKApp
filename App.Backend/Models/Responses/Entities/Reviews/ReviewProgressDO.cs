// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Reviews;

/// <summary>
/// Progress of one review kind within a round.
/// </summary>
public class ReviewVariantProgressDO
{
    [Required]
    public ReviewKinds Kind { get; set; }

    /// <summary>
    /// How many slots of this kind the round requires.
    /// </summary>
    [Required]
    public int Required { get; set; }

    [Required]
    public int Finished { get; set; }

    [Required]
    public int Active { get; set; }

    /// <summary>
    /// Finished slots whose verdict was a pass.
    /// </summary>
    [Required]
    public int Passed { get; set; }

    /// <summary>
    /// Finished slots whose verdict was a fail.
    /// </summary>
    [Required]
    public int Failed { get; set; }
}

/// <summary>
/// A single review slot of a round.
/// </summary>
public class ReviewSlotDO
{
    public ReviewSlotDO() { }

    public ReviewSlotDO(Review review)
    {
        ReviewId = review.Id;
        Kind = review.Kind;
        State = review.State;
        Reviewer = review.Reviewer;
        StartedAt = review.StartedAt;
        FinishedAt = review.FinishedAt;
        Passed = review.Passed;
    }

    [Required]
    public Guid ReviewId { get; set; }

    [Required]
    public ReviewKinds Kind { get; set; }

    [Required]
    public ReviewState State { get; set; }

    public UserBriefDO? Reviewer { get; set; }

    public DateTimeOffset? StartedAt { get; set; }
    
    public DateTimeOffset? FinishedAt { get; set; }

    public bool? Passed { get; set; }
}

/// <summary>
/// One evaluation attempt of a user project.
/// </summary>
public class ReviewRoundDO : BaseEntityDO<ReviewRound>
{
    public ReviewRoundDO() { }

    public ReviewRoundDO(ReviewRound round) : base(round)
    {
        UserProjectId = round.UserProjectId;
        Number = round.Attempt;
        State = round.State;
        RubricId = round.RubricId;
        Ref = round.Ref;
        Sha = round.Sha;
        RequestedById = round.RequestedById;
        ClosedAt = round.ClosedAt;
        Slots = [.. round.Reviews.OrderBy(r => r.CreatedAt).Select(r => new ReviewSlotDO(r))];
    }

    [Required]
    public Guid UserProjectId { get; set; }

    /// <summary>
    /// 1-based attempt number.
    /// </summary>
    [Required]
    public int Number { get; set; }

    [Required]
    public ReviewRoundState State { get; set; }

    [Required]
    public Guid RubricId { get; set; }

    [Required]
    public string Ref { get; set; } = null!;

    /// <summary>
    /// The exact commit this round evaluated. Null for rounds created before SHAs were recorded.
    /// </summary>
    [Required]
    public string Sha { get; set; } = null!;

    [Required]
    public Guid RequestedById { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    [Required]
    public IEnumerable<ReviewSlotDO> Slots { get; set; } = [];
}

/// <summary>
/// Data object representing the progress of reviews for a user project and rubric.
/// This includes the current round and the breakdown of review variants within it.
/// </summary>
public class ReviewProgressDO
{
    public ReviewProgressDO() { }

    public ReviewProgressDO(Rubric rubric)
    {
        Rubric = rubric;
    }

    [Required]
    public RubricLightDO Rubric { get; set; } = null!;

    /// <summary>
    /// The open round, or the most recent one if none is open. Null if the project was never evaluated.
    /// </summary>
    public ReviewRoundDO? Round { get; set; }

    [Required]
    public IEnumerable<ReviewVariantProgressDO> Variants { get; set; } = [];
}
