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

    public async Task<Review> RequestReviewAsync(Guid userProjectId, Guid rubricId, Guid initiatorId, CancellationToken token = default)
    {
        // 1. Verify state of user project
        var userProject = await _context.UserProjects
            .Include(up => up.GitInfo)
            .Include(up => up.Reviews)
            .FirstOrDefaultAsync(up => up.Id == userProjectId)
            ?? throw new ServiceException(404, "User project not found.");

        ServiceException.ThrowIf(userProject.GitInfo is null, "Project has nothing submitted for review.");
        var reviewable = userProject.State is EntityObjectState.Awaiting or EntityObjectState.Completed;
        ServiceException.ThrowIf(reviewable is false, "Project is not in a reviewable state.");

        // 2. Validate rubric being assignable to this project can actually be used to review this project
        var rubric = await _context.Rubrics
            .Include(r => r.GitInfo)
            .FirstOrDefaultAsync(r => r.Id == rubricId && r.Enabled)
            ?? throw new ServiceException(404, "Rubric not found or is disabled.");

        var members = await _context.Members
            .Where(m => m.EntityType == MemberEntityType.UserProject
                     && m.EntityId == userProjectId
                     && m.LeftAt == null)
            .ToListAsync(token);

        // 3. Common sense preconditions based on review kind
        var member = members.FirstOrDefault(m => m.UserId == initiatorId);
        var isLeader = member?.Role is MemberRole.Leader;

        if (rubric.ReviewVariant is ReviewKinds.Auto) // TODO: Implement auto review flow and remove this check
            throw new ServiceException(501, "Auto reviews are not supported yet.");
        if (rubric.ReviewVariant is ReviewKinds.Self && member is null)
            throw new ServiceException(422, "Only project members can request a self review.");
        if (rubric.ReviewVariant is ReviewKinds.Peer && (member is null || !isLeader))
            throw new ServiceException(422, "Only team leaders can request a peer review.");
        if (rubric.ReviewVariant is ReviewKinds.Async && (member is null || !isLeader))
            throw new ServiceException(422, "Only team leaders can request an async review.");

        // 4. Run additional business rules
        var initiator = await _context.Users.FirstOrDefaultAsync(u => u.Id == initiatorId)
            ?? throw new ServiceException(404, "Initiator not found.");
        var result = await rules.CanRequestReviewAsync(rubric, initiator, userProject, token);
        if (!result.IsSuccess)
            throw new ServiceException(string.Join("; ", result.Reasons));

        // 5. Create the review
        var review = new Review
        {
            RubricId = rubricId,
            State = ReviewState.Pending,
            Kind = rubric.ReviewVariant,
            UserProjectId = userProjectId,
            ReviewerId = rubric.ReviewVariant is ReviewKinds.Self ? initiatorId : null,
        };

        _dbSet.Add(review);
        await _context.SaveChangesAsync(token);
        return review;
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
        if (review.Kind is ReviewKinds.Peer or ReviewKinds.Async)
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
            review.ReviewerId is null && review.Kind is not ReviewKinds.Auto,
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