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

public class ReviewService(DatabaseContext ctx, IRuleService rules) : BaseService<Review>(ctx), IReviewService
{
    private readonly DatabaseContext _context = ctx;

    /// <inheritdoc />
    public async Task<Review> RequestReviewAsync(Guid userProjectId, Guid rubricId, Guid initiatorId, ReviewVariant[] variants, CancellationToken token = default)
    {
        ServiceException.ThrowIf(variants.Length == 0, "At least one review variant must be requested.");

        // 1. Load all required entities upfront
        var userProject = await _context.UserProjects
            .Include(up => up.GitInfo)
            .Include(up => up.Members.Where(m => m.LeftAt == null))
            .Include(up => up.Reviews)
            .FirstOrDefaultAsync(up => up.Id == userProjectId, token)
            ?? throw new ServiceException(404, "User project not found.");

        var rubric = await _context.Rubrics
            .Include(r => r.GitInfo)
            .FirstOrDefaultAsync(r => r.Id == rubricId && r.Enabled, token)
            ?? throw new ServiceException(404, "Rubric not found or is disabled.");

        var initiator = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == initiatorId, token)
            ?? throw new ServiceException(404, "Initiator not found.");

        // 2. Static preconditions (project state)
        ServiceException.ThrowIf(userProject.GitInfo is null, "Project has nothing submitted for review.");
        ServiceException.ThrowIf(
            userProject.State is not (EntityObjectState.Awaiting or EntityObjectState.Completed),
            "Project is not in a reviewable state."
        );

        // 3. Validate each requested variant and create reviews
        var createdReviews = new List<Review>();
        foreach (var variant in variants.Distinct())
        {
            ValidateVariantSupported(rubric, variant);
            ValidateNoDuplicateReview(userProject, rubric, variant);
            ValidateVariantPreconditions(userProject, rubric, initiator, variant);

            // 4. Rule engine checks (dynamic rules configured in rubric)
            var ruleResult = await rules.AbleToRequestReviewAsync(rubric, initiator, userProject, token);
            if (!ruleResult.IsEligible)
                throw new ServiceException(string.Join("; ", ruleResult.Reasons));

            // 5. Create the review
            var review = new Review
            {
                Kind = variant,
                State = ReviewState.Pending,
                UserProjectId = userProjectId,
                RubricId = rubricId,
                ReviewerId = variant == ReviewVariant.Self ? initiatorId : null
            };

            _context.Set<Review>().Add(review);
            createdReviews.Add(review);
        }

        await _context.SaveChangesAsync(token);
        return createdReviews.First();
    }

    private static void ValidateVariantSupported(Rubric rubric, ReviewVariant variant)
    {
        var requiredKind = variant switch
        {
            ReviewVariant.Self => ReviewKinds.Self,
            ReviewVariant.Peer => ReviewKinds.Peer,
            ReviewVariant.Async => ReviewKinds.Async,
            ReviewVariant.Auto => ReviewKinds.Auto,
            _ => throw new ServiceException($"Unknown review variant: {variant}")
        };

        ServiceException.ThrowIf(
            !rubric.SupportedReviewKinds.HasFlag(requiredKind),
            $"Rubric does not support {variant} reviews."
        );
    }

    private static void ValidateNoDuplicateReview(UserProject userProject, Rubric rubric, ReviewVariant variant)
    {
        var hasDuplicate = userProject.Reviews.Any(r =>
            r.RubricId == rubric.Id &&
            r.Kind == variant &&
            r.State != ReviewState.Finished
        );

        ServiceException.ThrowIf(hasDuplicate, $"A pending {variant} review already exists for this rubric.");
    }

    private static void ValidateVariantPreconditions(UserProject userProject, Rubric rubric, User initiator, ReviewVariant variant)
    {
        switch (variant)
        {
            case ReviewVariant.Self:
                // Self-review: initiator must be an active member
                var isMember = userProject.Members.Any(m => m.UserId == initiator.Id);
                ServiceException.ThrowIf(!isMember, "Only project members can request a self-review.");
                break;

            case ReviewVariant.Peer:
                // Peer review: initiator must be the leader
                var isLeader = userProject.Members.Any(m =>
                    m.UserId == initiator.Id && m.Role == UserProjectRole.Leader);
                ServiceException.ThrowIf(!isLeader, "Only the project leader can request a peer review.");
                break;

            case ReviewVariant.Async:
                // Async review: initiator must be a member (any role)
                var isAsyncMember = userProject.Members.Any(m => m.UserId == initiator.Id);
                ServiceException.ThrowIf(!isAsyncMember, "Only project members can request an async review.");
                break;

            case ReviewVariant.Auto:
                // Auto review: rubric must have git info for running tests
                ServiceException.ThrowIf(
                    rubric.GitInfo is null,
                    "Rubric has no test repository configured for auto reviews."
                );
                break;
        }
    }

    /// <inheritdoc />
    public async Task<Review> AssignReviewerAsync(Guid reviewId, Guid reviewerId, CancellationToken token = default)
    {
        var review = await _context.Reviews
            .Include(r => r.Rubric)
            .Include(r => r.UserProject)
                .ThenInclude(up => up.Members.Where(m => m.LeftAt == null))
            .FirstOrDefaultAsync(r => r.Id == reviewId, token)
            ?? throw new ServiceException(404, "Review not found.");

        ServiceException.ThrowIf(review.State != ReviewState.Pending, "Review must be pending to assign a reviewer.");

        var reviewer = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == reviewerId, token)
            ?? throw new ServiceException(404, "Reviewer not found.");

        // Check reviewer eligibility via rule engine
        var ruleResult = await rules.AbleToReviewAsync(review.Rubric, reviewer, review.UserProject, token);
        if (!ruleResult.IsEligible)
            throw new ServiceException(403, string.Join("; ", ruleResult.Reasons));

        // Peer/Async reviews require reviewer to be a team member
        if (review.Kind is ReviewVariant.Peer or ReviewVariant.Async)
        {
            var isMember = review.UserProject.Members.Any(m => m.UserId == reviewerId);
            ServiceException.ThrowIf(!isMember, $"{review.Kind} reviews must be performed by a team member.");
        }

        review.ReviewerId = reviewerId;
        await _context.SaveChangesAsync(token);
        return review;
    }

    /// <inheritdoc />
    public async Task<Review> StartReviewAsync(Guid reviewId, CancellationToken token = default)
    {
        var review = await _dbSet.FirstOrDefaultAsync(r => r.Id == reviewId, token)
            ?? throw new ServiceException(404, "Review not found.");

        ServiceException.ThrowIf(review.State != ReviewState.Pending, "Review must be pending to start.");
        ServiceException.ThrowIf(
            review.ReviewerId is null && review.Kind != ReviewVariant.Auto,
            "Review must have an assigned reviewer before starting."
        );

        review.State = ReviewState.InProgress;
        await _context.SaveChangesAsync(token);
        return review;
    }

    /// <inheritdoc />
    public async Task<Review> CompleteReviewAsync(Guid reviewId, CancellationToken token = default)
    {
        var review = await _dbSet.FirstOrDefaultAsync(r => r.Id == reviewId, token)
            ?? throw new ServiceException(404, "Review not found.");

        ServiceException.ThrowIf(review.State != ReviewState.InProgress, "Review must be in progress to complete.");

        review.State = ReviewState.Finished;
        await _context.SaveChangesAsync(token);
        return review;
    }

    /// <inheritdoc />
    public async Task CancelReviewAsync(Guid reviewId, CancellationToken token = default)
    {
        var review = await _dbSet.FirstOrDefaultAsync(r => r.Id == reviewId, token)
            ?? throw new ServiceException(404, "Review not found.");

        ServiceException.ThrowIf(review.State != ReviewState.Pending, "Only pending reviews can be canceled.");

        _dbSet.Remove(review);
        await _context.SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Review>> GetPendingReviewsAsync(Guid userProjectId, CancellationToken token = default)
    {
        return await _dbSet
            .Where(r => r.UserProjectId == userProjectId && r.State == ReviewState.Pending)
            .Include(r => r.Rubric)
            .Include(r => r.Reviewer)
            .ToListAsync(token);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Review>> GetReviewerAssignmentsAsync(Guid reviewerId, CancellationToken token = default)
    {
        return await _dbSet
            .Where(r => r.ReviewerId == reviewerId && r.State != ReviewState.Finished)
            .Include(r => r.Rubric)
            .Include(r => r.UserProject)
            .ToListAsync(token);
    }
}
