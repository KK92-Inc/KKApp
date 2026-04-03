// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;
using App.Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class ReviewService(DatabaseContext ctx, IRuleService rules) : BaseService<Review>(ctx), IReviewService
{
    private readonly DatabaseContext _context = ctx;

    /// <inheritdoc />
    public async Task<IEnumerable<Review>> RequestReviewAsync(
        Guid userProjectId, Guid rubricId, Guid initiatorId,
        ReviewVariant[] variants, CancellationToken token = default)
    {
        ServiceException.ThrowIf(variants.Length == 0, "At least one review variant must be requested.");

        // 1. Load all required entities upfront.
        //    Members are no longer a navigation property — load separately.
        var userProject = await _context.UserProjects
            .Include(up => up.GitInfo)
            .Include(up => up.Reviews)
            .FirstOrDefaultAsync(up => up.Id == userProjectId, token)
            ?? throw new ServiceException(404, "User project not found.");

        var activeMembers = await _context.Members
            .Where(m => m.EntityType == MemberEntityType.UserProject
                     && m.EntityId == userProjectId
                     && m.LeftAt == null)
            .ToListAsync(token);

        var rubric = await _context.Rubrics
            .Include(r => r.GitInfo)
            .FirstOrDefaultAsync(r => r.Id == rubricId && r.Enabled, token)
            ?? throw new ServiceException(404, "Rubric not found or is disabled.");

        var initiator = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == initiatorId, token)
            ?? throw new ServiceException(404, "Initiator not found.");

        // 2. Static preconditions
        ServiceException.ThrowIf(userProject.GitInfo is null, "Project has nothing submitted for review.");
        ServiceException.ThrowIf(
            userProject.State is not (EntityObjectState.Awaiting or EntityObjectState.Completed),
            "Project is not in a reviewable state."
        );

        // 3. Validate each requested variant and create reviews
        var createdReviews = new List<Review>();
        foreach (var variant in variants.Distinct())
        {
            var member = activeMembers.FirstOrDefault(m => m.UserId == initiatorId);
            switch (variant)
            {
                case ReviewVariant.Self:
                case ReviewVariant.Peer:
                    ServiceException.ThrowIf(
                        member?.Role is not MemberRole.Leader,
                        "Only the project leader can request a peer or self review."
                    );
                    break;
                case ReviewVariant.Async:
                    ServiceException.ThrowIf(
                        member is not null,
                        "Only non-members can request an async review."
                    );
                    break;
                case ReviewVariant.Auto:
                    throw new ServiceException(501, "Auto reviews are not supported yet.");
            }

            // 4. Rule engine checks
            var ruleResult = await rules.CanRequestReviewAsync(rubric, initiator, userProject, token);
            if (!ruleResult.IsSuccess)
                throw new ServiceException(string.Join("; ", ruleResult.Reasons));

            // 5. Create the review
            var review = new Review
            {
                Kind = variant,
                State = ReviewState.Pending,
                UserProjectId = userProjectId,
                RubricId = rubricId,
                ReviewerId = variant == ReviewVariant.Self ? initiatorId : null,
            };

            _dbSet.Add(review);
            createdReviews.Add(review);
        }

        await _context.SaveChangesAsync(token);
        return createdReviews;
    }

    /// <inheritdoc />
    public async Task<Review> AssignReviewerAsync(Guid reviewId, Guid reviewerId, CancellationToken token = default)
    {
        var review = await _context.Reviews
            .Include(r => r.Rubric)
            .Include(r => r.UserProject)
            .FirstOrDefaultAsync(r => r.Id == reviewId, token)
            ?? throw new ServiceException(404, "Review not found.");

        ServiceException.ThrowIf(review.State is not ReviewState.Pending, "Review must be pending to assign a reviewer.");

        var reviewer = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == reviewerId, token)
            ?? throw new ServiceException(404, "Reviewer not found.");

        var ruleResult = await rules.CanReviewAsync(review.Rubric, reviewer, review.UserProject, token);
        if (!ruleResult.IsSuccess)
            throw new ServiceException(403, string.Join("; ", ruleResult.Reasons));

        // Peer/Async: reviewer must be an active team member — query tbl_members directly
        if (review.Kind is ReviewVariant.Peer or ReviewVariant.Async)
        {
            var isMember = await _context.Members.AnyAsync(m =>
                m.EntityType == MemberEntityType.UserProject &&
                m.EntityId == review.UserProjectId &&
                m.UserId == reviewerId &&
                m.LeftAt == null, token);

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

        ServiceException.ThrowIf(review.State is not ReviewState.Pending, "Review must be pending to start.");
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

        ServiceException.ThrowIf(review.State is not ReviewState.InProgress, "Review must be in progress to complete.");

        review.State = ReviewState.Finished;
        await _context.SaveChangesAsync(token);
        return review;
    }

    /// <inheritdoc />
    public async Task CancelReviewAsync(Guid reviewId, CancellationToken token = default)
    {
        var review = await _dbSet.FirstOrDefaultAsync(r => r.Id == reviewId, token)
            ?? throw new ServiceException(404, "Review not found.");

        ServiceException.ThrowIf(review.State is not ReviewState.Pending, "Only pending reviews can be canceled.");

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