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
    #region ReviewPushAndPull

    /// <summary>
    /// "Pulls" for reviews meaning it will open a new evaluation round for a
    /// user project and create review "slots" from the rubric.
    /// 
    /// The session moves to <see cref="Enums.EntityObjectState.Awaiting"/> and
    /// locks the repository.
    /// 
    /// Validates that:
    /// - The user project exists, has something submitted and is not completed or inactive
    /// - The project has no open round already (one open round at a time)
    /// </summary>
    /// <param name="userProjectId"></param>
    /// <param name="initiatorId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<ReviewRound> PullReviewAsync(Guid userProjectId, Guid initiatorId, CancellationToken token = default);

    /// <summary>
    /// Directly gives a review for a user project as the specified reviewer.
    /// 
    /// If the project has an open round with a "free slot" of the requested kind,
    /// the slot is claimed (first come, first served) and the review counts towards the round.
    /// 
    /// Otherwise an advisory review is created (no round): it carries feedback but never counts
    /// for or against the project, even when the project is completed.
    /// 
    /// The review is <see cref="Enums.ReviewState.Pending"/> and attached to the reviewer, ready to be
    /// started via the normal start/complete lifecycle. There is no scheduling, a claimed review that
    /// isn't started in time is released again by the cleanup job.
    /// 
    /// Only <see cref="Enums.ReviewKinds.Peer"/> and <see cref="Enums.ReviewKinds.Async"/>
    /// are supported. The reviewed ref is always the project's default (master) branch.
    /// </summary>
    /// <param name="userProjectId">The user project being reviewed.</param>
    /// <param name="kind">The kind of review being given (Peer or Async).</param>
    /// <param name="reviewerId">Only null if the kind is Auto, else it must be specified.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The claimed slot or the created advisory review (check <see cref="Review.RoundId"/>).</returns>
    public Task<Review> PushReviewAsync(
        Guid userProjectId,
        ReviewKinds kind,
        Guid reviewerId,
        CancellationToken token = default
    );

    #endregion

    #region ReviewActions

    /// <summary>
    /// Assigns a reviewer to an unclaimed pending slot of an open round (first come, first served).
    /// Validates that the reviewer meets the rubric's eligibility requirements and doesn't
    /// already hold another slot in the same round.
    /// </summary>
    /// <param name="reviewId">The review to assign.</param>
    /// <param name="reviewerId">The user to assign as reviewer.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The updated review.</returns>
    public Task<Review> AssignReviewerAsync(Guid reviewId, Guid reviewerId, CancellationToken token = default);

    /// <summary>
    /// Starts a pending review.
    /// </summary>
    /// <param name="reviewId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Review> StartReviewAsync(Guid reviewId, CancellationToken token = default);

    /// <summary>
    /// Stops / cancels a review and invalidates it.
    /// </summary>
    /// <param name="reviewId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Review> StopReviewAsync(Guid reviewId, CancellationToken token = default);

    /// <summary>
    /// Completes / finishes a review.
    /// </summary>
    /// <param name="reviewId"></param>
    /// <param name="passed">Was this project good enough to consider it passing ?</param>
    /// <param name="annotations">Any annotations that were given for this review</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Review> CompleteReviewAsync(Guid reviewId, bool passed, IEnumerable<Annotation> annotations, CancellationToken token = default);

    #endregion

    #region RoundActions

    /// <summary>
    /// Finds a round by id, including its slots.
    /// </summary>
    /// <param name="roundId">The round to cancel.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The round.</returns>
    public Task<ReviewRound?> FindRoundByIdAsync(Guid roundId, CancellationToken token = default);

    /// <summary>
    /// Gets all evaluation rounds of a user project.
    /// </summary>
    /// <param name="userProjectId">The user project being reviewed.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A list of all the rounds.</returns>
    public Task<IEnumerable<ReviewRound>> GetRoundsAsync(Guid userProjectId, CancellationToken token = default);

    /// <summary>
    /// Cancels an ongoing / open round. Will not proceed if there is a currently started review.
    /// If any reviews have not yet started it will cancel them. Completed reviews will not be affected.
    /// 
    /// Additionally it will lock the session for 1 hour to prevent abuse.
    /// </summary>
    /// <param name="roundId">The round to cancel.</param>
    /// <param name="token">Cancellation token.</param>
    public Task<ReviewRound> CancelRoundAsync(Guid roundId, CancellationToken token = default);

    #endregion

    #region Annotations

    // public Task<IEnumerable<Annotation>> GetAnnotationsAsync(Guid reviewId, CancellationToken token = default);

    #endregion

}