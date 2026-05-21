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

    public async Task<IEnumerable<Review>> RequestReviewAsync(Guid userProjectId, Guid rubricId, Guid initiatorId, CancellationToken token = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // 1. Verify state of user project
            var userProject = await _context.UserProjects
                .Include(up => up.GitInfo)
                .Include(up => up.Reviews)
                .FirstOrDefaultAsync(up => up.Id == userProjectId, ct)
                ?? throw new ServiceException(404, "User project not found.");

            ServiceException.ThrowIf(userProject.GitInfo is null, "Project has nothing submitted for review.");

            // 1.5: Lock the project into an awaiting state (unless it is completed, then it doesn't matter)
            switch (userProject.State)
            {
                case EntityObjectState.Awaiting:
                    throw new ServiceException(422, "Project is already awaiting review.");
                case EntityObjectState.Inactive:
                    throw new ServiceException(422, "Project is currently inactive and cannot be reviewed.");
                case EntityObjectState.Completed:
                    break;
                case EntityObjectState.Active:
                    userProject.State = EntityObjectState.Awaiting;
                    await _context.SaveChangesAsync(ct);
                    break;
                default: throw new ServiceException(500, "Invalid project state.");
            }

            // 2. Resolve rubric — prefer project-specific, fall back to wildcard
            var rubric = await _context.Rubrics
                .Include(r => r.GitInfo)
                .Include(r => r.Variants)
                .Where(r => r.Enabled && r.Id == rubricId
                         && (r.ProjectId == userProject.ProjectId || r.ProjectId == null))
                .OrderByDescending(r => r.ProjectId != null)
                .FirstOrDefaultAsync(ct)
                ?? throw new ServiceException(404, "No rubric available for this project.");

            var variants = rubric.Variants.Where(v => v.Count > 0).ToList();
            if (variants.Count is 0)
                throw new ServiceException(422, "Rubric has no active review kinds configured.");

            // 3. Common sense preconditions per active variant
            var members = await _context.Members
                .Where(m => m.EntityType == MemberEntityType.UserProject
                         && m.EntityId == userProjectId
                         && m.LeftAt == null)
                .ToListAsync(ct);

            var member = members.FirstOrDefault(m => m.UserId == initiatorId);
            var isLeader = member?.Role is MemberRole.Leader;

            foreach (var variant in variants)
            {
                switch (variant.Kind)
                {
                    case ReviewKinds.Self when member is null:
                        throw new ServiceException(422, "Only project members can request a self review.");
                    case ReviewKinds.Peer when member is null || !isLeader:
                    case ReviewKinds.Async when member is null || !isLeader:
                        throw new ServiceException(422, "Only team leaders can request a peer or async review.");
                }
            }

            // 4. Run additional business rules
            var initiator = await _context.Users.FirstOrDefaultAsync(u => u.Id == initiatorId, ct)
                ?? throw new ServiceException(404, "Initiator not found.");
            var result = await rules.CanRequestReviewAsync(rubric, initiator, userProject, ct);
            if (!result.IsSuccess)
                throw new ServiceException(string.Join("; ", result.Reasons));

            // 5. Create N review rows per kind based on RequiredCount
            var reviews = variants
                .SelectMany(variant => Enumerable.Range(0, variant.Count)
                    .Select(_ => new Review
                    {
                        RubricId = rubricId,
                        Kind = variant.Kind,
                        State = ReviewState.Pending,
                        UserProjectId = userProjectId,
                        RubricRef = "master", // TODO: Query the submitted branch somehow
                        ReviewerId = variant.Kind is ReviewKinds.Self ? initiatorId : null,
                    }))
                .ToList();

            _dbSet.AddRange(reviews);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return reviews;
        }, token);
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