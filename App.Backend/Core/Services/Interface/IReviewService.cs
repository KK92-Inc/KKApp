// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Models;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IReviewService : IDomainService<Review>
{
    /// <summary>
    /// Creates a review request for a user project.
    /// Validates that:
    /// - The user project exists and is in a valid state
    /// - The rubric supports the requested review kind
    /// - No duplicate review of the same kind exists
    /// - The reviewee meets the rubric's eligibility requirements
    /// </summary>
    /// <param name="userProjectId">The user project to be reviewed.</param>
    /// <param name="rubricId">The rubric to use for evaluation.</param>
    /// <param name="kind">The type of review (Self, Peer, Async, Auto).</param>
    /// <param name="requestingUserId">The user requesting the review.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The created review.</returns>
    Task<Review> RequestReviewAsync(
        Guid userProjectId,
        Guid rubricId,
        ReviewVariant kind,
        Guid requestingUserId,
        CancellationToken token = default
    );

    /// <summary>
    /// Assigns a reviewer to a pending review.
    /// Validates that the reviewer meets the rubric's eligibility requirements.
    /// </summary>
    /// <param name="reviewId">The review to assign.</param>
    /// <param name="reviewerId">The user to assign as reviewer.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The updated review.</returns>
    Task<Review> AssignReviewerAsync(
        Guid reviewId,
        Guid reviewerId,
        CancellationToken token = default
    );

    /// <summary>
    /// Starts a review, changing its state from Pending to InProgress.
    /// </summary>
    /// <param name="reviewId">The review to start.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The updated review.</returns>
    Task<Review> StartReviewAsync(Guid reviewId, CancellationToken token = default);

    /// <summary>
    /// Completes a review, changing its state to Finished.
    /// </summary>
    /// <param name="reviewId">The review to complete.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The updated review.</returns>
    Task<Review> CompleteReviewAsync(Guid reviewId, CancellationToken token = default);

    /// <summary>
    /// Gets all pending reviews for a user project.
    /// </summary>
    /// <param name="userProjectId">The user project ID.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>List of pending reviews.</returns>
    Task<IEnumerable<Review>> GetPendingReviewsAsync(Guid userProjectId, CancellationToken token = default);

    /// <summary>
    /// Gets reviews assigned to a specific reviewer.
    /// </summary>
    /// <param name="reviewerId">The reviewer's user ID.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>List of reviews assigned to the reviewer.</returns>
    Task<IEnumerable<Review>> GetReviewerAssignmentsAsync(Guid reviewerId, CancellationToken token = default);
}
