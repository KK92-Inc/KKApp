// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Models;

namespace App.Backend.Core.Services.Interface;

// ============================================================================

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
    /// <param name="initiatorId">The user requesting the review.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The created reviews based on the variants that the rubric supports.</returns>
    public Task<IEnumerable<Review>> RequestReviewAsync(
        Guid userProjectId,
        Guid initiatorId,
        string @ref,
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
    public Task<Review> AssignReviewerAsync(
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
    public Task<Review> StartReviewAsync(Guid reviewId, CancellationToken token = default);

    /// <summary>
    /// Cancels a pending review, removing it entirely.
    /// Only pending reviews can be canceled.
    /// </summary>
    /// <param name="reviewId">The review to cancel.</param>
    /// <param name="token">Cancellation token.</param>
    public Task CancelReviewAsync(Guid reviewId, CancellationToken token = default);

    /// <summary>
    /// Completes a review, changing its state to Finished.
    /// </summary>
    /// <param name="reviewId">The review to complete.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The updated review.</returns>
    public Task<Review> CompleteReviewAsync(Guid reviewId, CancellationToken token = default);

    /// <summary>
    /// Gets all pending reviews for a user project.
    /// </summary>
    /// <param name="userProjectId">The user project ID.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>List of pending reviews.</returns>
    public Task<IEnumerable<Review>> GetPendingReviewsAsync(Guid userProjectId, CancellationToken token = default);

    /// <summary>
    /// Gets reviews assigned to a specific reviewer.
    /// </summary>
    /// <param name="reviewerId">The reviewer's user ID.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>List of reviews assigned to the reviewer.</returns>
    public Task<IEnumerable<Review>> GetReviewerAssignmentsAsync(Guid reviewerId, CancellationToken token = default);

    /// <summary>
    /// Gets all annotations for a specific file in a review.
    /// </summary>
    /// <param name="reviewId">The review ID.</param>
    /// <param name="ref">The git ref.</param>
    /// <param name="filePath">The file path.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>List of annotations.</returns>
    public Task<IEnumerable<Annotation>> GetAnnotationsAsync(Guid reviewId, string filePath, CancellationToken token = default);

    /// <summary>
    /// Sets all annotations for a specific file in a review.
    /// </summary>
    /// <param name="reviewId">The review ID.</param>
    /// <param name="authorId">The author of the annotations.</param>
    /// <param name="ref">The git ref.</param>
    /// <param name="filePath">The file path.</param>
    /// <param name="annotations">The annotations to set.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>List of annotations.</returns>
    public Task<IEnumerable<Annotation>> SetAnnotationsAsync(
        Guid reviewId,
        Guid authorId,
        string filePath,
        IEnumerable<Annotation> annotations,
        CancellationToken token = default
    );
}
