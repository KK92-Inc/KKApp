// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class ReviewService(
    DatabaseContext ctx,
    IRuleService eligibilityService
) : BaseService<Review>(ctx), IReviewService
{
    private readonly DatabaseContext _context = ctx;
    private readonly IRuleService _eligibilityService = eligibilityService;

    /// <inheritdoc />
    public async Task<Review> RequestReviewAsync(
        Guid userProjectId,
        Guid rubricId,
        ReviewVariant kind,
        Guid requestingUserId,
        CancellationToken token = default)
    {
        // 1. Get the user project with its members
        var userProject = await _context.UserProjects
            .Include(up => up.Members)
            .Include(up => up.Project)
            .FirstOrDefaultAsync(up => up.Id == userProjectId, token)
            ?? throw new ServiceException(404, "User project not found");

        // 2. Validate the user project state - must be ready for review
        if (userProject.State != EntityObjectState.Awaiting)
        {
            throw new ServiceException(
                422,
                "User project is not ready for review",
                $"Current state: {userProject.State}. Expected: Awaiting");
        }

        // 3. Get the rubric
        var rubric = await _context.Rubrics
            .FirstOrDefaultAsync(r => r.Id == rubricId && r.Enabled, token)
            ?? throw new ServiceException(404, "Rubric not found or not enabled");

        // 4. Check if the rubric supports this review kind
        var kindFlag = kind switch
        {
            ReviewVariant.Self => ReviewKinds.Self,
            ReviewVariant.Peer => ReviewKinds.Peer,
            ReviewVariant.Async => ReviewKinds.Async,
            ReviewVariant.Auto => ReviewKinds.Auto,
            _ => throw new ServiceException(422, $"Unknown review kind: {kind}")
        };

        if (!rubric.SupportedReviewKinds.HasFlag(kindFlag))
        {
            throw new ServiceException(
                422,
                $"Rubric does not support {kind} reviews",
                $"Supported kinds: {rubric.SupportedReviewKinds}");
        }

        // 5. Check for duplicate review of the same kind
        var existingReview = await _context.Reviews
            .Where(r => r.UserProjectId == userProjectId
                        && r.RubricId == rubricId
                        && r.Kind == kind
                        && r.State != ReviewState.Finished)
            .FirstOrDefaultAsync(token);

        if (existingReview is not null)
        {
            throw new ServiceException(
                409,
                $"A {kind} review for this project with this rubric already exists",
                $"Existing review ID: {existingReview.Id}");
        }

        // 6. Get the requesting user and check reviewee eligibility
        var requestingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == requestingUserId, token)
            ?? throw new ServiceException(404, "Requesting user not found");

        var eligibility = await _eligibilityService.AbleToReviewAsync(
            rubric, requestingUser, userProject, token);

        if (!eligibility.IsEligible)
        {
            throw new ServiceException(
                403,
                "User is not eligible to request this review",
                string.Join("; ", eligibility.Reasons));
        }

        // 7. Create the review
        var review = new Review
        {
            UserProjectId = userProjectId,
            RubricId = rubricId,
            Kind = kind,
            State = ReviewState.Pending,
            ReviewerId = null
        };

        // For Self reviews, automatically assign the requesting user
        if (kind == ReviewVariant.Self)
        {
            review.ReviewerId = requestingUserId;
        }

        return await CreateAsync(review, token);
    }

    /// <inheritdoc />
    public async Task<Review> AssignReviewerAsync(
        Guid reviewId,
        Guid reviewerId,
        CancellationToken token = default)
    {
        // 1. Get the review with related data
        var review = await _context.Reviews
            .Include(r => r.Rubric)
            .Include(r => r.UserProject)
                .ThenInclude(up => up.Members)
            .FirstOrDefaultAsync(r => r.Id == reviewId, token)
            ?? throw new ServiceException(404, "Review not found");

        // 2. Validate review state
        if (review.State != ReviewState.Pending)
        {
            throw new ServiceException(
                422,
                "Review is not in pending state",
                $"Current state: {review.State}");
        }

        // 3. Get the reviewer
        var reviewer = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == reviewerId, token)
            ?? throw new ServiceException(404, "Reviewer not found");

        // 4. Check reviewer eligibility
        // var eligibility = await _eligibilityService.AbleToRequestReviewAsync(
        //     review.Rubric, review.UserProject, reviewer, token);

        // if (!eligibility.IsEligible)
        // {
        //     throw new ServiceException(
        //         403,
        //         "User is not eligible to be a reviewer",
        //         string.Join("; ", eligibility.Reasons));
        // }

        // 5. For Peer and Self reviews, ensure reviewer is a team member
        if (review.Kind == ReviewVariant.Peer || review.Kind == ReviewVariant.Self)
        {
            var isMember = review.UserProject.Members.Any(m => m.UserId == reviewerId);
            if (review.Kind == ReviewVariant.Self && !isMember)
            {
                throw new ServiceException(
                    403,
                    "Self reviews must be performed by a team member");
            }
        }

        // 6. Assign the reviewer
        review.ReviewerId = reviewerId;
        review.Reviewer = reviewer;

        await UpdateAsync(review, token);
        return review;
    }

    /// <inheritdoc />
    public async Task<Review> StartReviewAsync(Guid reviewId, CancellationToken token = default)
    {
        var review = await _dbSet
            .FirstOrDefaultAsync(r => r.Id == reviewId, token)
            ?? throw new ServiceException(404, "Review not found");

        if (review.State != ReviewState.Pending)
        {
            throw new ServiceException(
                422,
                "Review must be in pending state to start",
                $"Current state: {review.State}");
        }

        if (review.ReviewerId is null && review.Kind != ReviewVariant.Auto)
        {
            throw new ServiceException(
                422,
                "Review must have an assigned reviewer before starting");
        }

        review.State = ReviewState.InProgress;
        await UpdateAsync(review, token);
        return review;
    }

    /// <inheritdoc />
    public async Task<Review> CompleteReviewAsync(Guid reviewId, CancellationToken token = default)
    {
        var review = await _dbSet
            .FirstOrDefaultAsync(r => r.Id == reviewId, token)
            ?? throw new ServiceException(404, "Review not found");

        if (review.State != ReviewState.InProgress)
        {
            throw new ServiceException(
                422,
                "Review must be in progress to complete",
                $"Current state: {review.State}");
        }

        review.State = ReviewState.Finished;
        await UpdateAsync(review, token);
        return review;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Review>> GetPendingReviewsAsync(
        Guid userProjectId,
        CancellationToken token = default)
    {
        return await _dbSet
            .Where(r => r.UserProjectId == userProjectId && r.State == ReviewState.Pending)
            .Include(r => r.Rubric)
            .Include(r => r.Reviewer)
            .ToListAsync(token);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Review>> GetReviewerAssignmentsAsync(
        Guid reviewerId,
        CancellationToken token = default)
    {
        return await _dbSet
            .Where(r => r.ReviewerId == reviewerId && r.State != ReviewState.Finished)
            .Include(r => r.Rubric)
            .Include(r => r.UserProject)
            .ToListAsync(token);
    }
}
