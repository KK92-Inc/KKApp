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
    public ReviewKinds Kind { get; init; }

    /// <summary>
    /// How many slots of this kind the round requires.
    /// </summary>
    [Required]
    public int Required { get; init; }

    [Required]
    public int Finished { get; init; }

    [Required]
    public int Active { get; init; }

    /// <summary>
    /// Finished slots whose verdict was a pass.
    /// </summary>
    [Required]
    public int Passed { get; init; }

    /// <summary>
    /// Finished slots whose verdict was a fail.
    /// </summary>
    [Required]
    public int Failed { get; init; }
}

/// <summary>
/// A single review slot of a round.
/// </summary>
public class ReviewSlotDO(Review review)
{
    [Required]
    public Guid ReviewId { get; init; } = review.Id;

    [Required]
    public ReviewKinds Kind { get; init; } = review.Kind;

    [Required]
    public ReviewState State { get; init; } = review.State;

    public Guid? ReviewerId { get; init; } = review.ReviewerId;

    public bool? Passed { get; init; } = review.Passed;
}

/// <summary>
/// One evaluation attempt of a user project.
/// </summary>
public class ReviewRoundDO(ReviewRound round) : BaseEntityDO<ReviewRound>(round)
{
    [Required]
    public Guid UserProjectId { get; init; } = round.UserProjectId;

    /// <summary>
    /// 1-based attempt number.
    /// </summary>
    [Required]
    public int Number { get; init; } = round.Attempt;

    [Required]
    public ReviewRoundState State { get; init; } = round.State;

    [Required]
    public Guid RubricId { get; init; } = round.RubricId;

    [Required]
    public string Ref { get; init; } = round.Ref;

    public Guid? RequestedById { get; init; } = round.RequestedById;

    public DateTimeOffset? ClosedAt { get; init; } = round.ClosedAt;

    [Required]
    public IEnumerable<ReviewSlotDO> Slots { get; init; } =
        [.. round.Reviews.OrderBy(r => r.CreatedAt).Select(r => new ReviewSlotDO(r))];
}

/// <summary>
/// Data object representing the progress of reviews for a user project and rubric.
/// This includes the current round and the breakdown of review variants within it.
/// </summary>
public class ReviewProgressDO(Rubric rubric)
{
    [Required]
    public RubricLightDO Rubric { get; init; } = rubric;

    /// <summary>
    /// The open round, or the most recent one if none is open. Null if the project was never evaluated.
    /// </summary>
    public ReviewRoundDO? Round { get; init; }

    [Required]
    public IEnumerable<ReviewVariantProgressDO> Variants { get; init; } = [];
}
