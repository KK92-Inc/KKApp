// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Reviews;

/// <summary>
/// Data object representing a review.
/// </summary>
public class ReviewDO(Review review) : BaseEntityDO<Review>(review)
{
    [Required]
    public ReviewVariant Kind { get; set; } = review.Kind;

    [Required]
    public ReviewState State { get; set; } = review.State;

    /// <summary>
    /// The reviewer's user ID, if assigned.
    /// </summary>
    public Guid? ReviewerId { get; set; } = review.ReviewerId;

    [Required]
    public Guid UserProjectId { get; set; } = review.UserProjectId;

    /// <summary>
    /// The user performing the review, if assigned.
    /// </summary>
    public UserLightDO? Reviewer { get; set; } = review.Reviewer;

    public static implicit operator ReviewDO?(Review? review) =>
        review is null ? null : new(review);
}
