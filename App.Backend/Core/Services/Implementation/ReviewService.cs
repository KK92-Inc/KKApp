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
using App.Backend.Core.Query;
using System.Linq.Expressions;

namespace App.Backend.Core.Services.Implementation;

public class ReviewService(DatabaseContext context, IRuleService rules) : BaseService<Review>(context), IReviewService
{
    // ============================================================================
    // Public API (Core Actions)
    // ============================================================================

    public override async Task<PaginatedList<Review>> GetAllAsync(ISorting sorting, IPagination pagination, CancellationToken token = default, params Expression<Func<Review, bool>>?[] filters)
    {
        return await filters
            .Where(f => f is not null)
            .Aggregate(_dbSet.AsQueryable(), (c, filter) => c.Where(filter!))
            .Sort(sorting)
            .Include(r => r.Rubric)
            .Include(r => r.UserProject)
            .ThenInclude(up => up.GitInfo)
            .ThenInclude(up => up.Projects)
            .Include(r => r.Reviewer)
            .PaginateAsync(pagination, token);
    }

    public async Task<IEnumerable<Review>> RequestReviewAsync(Guid userProjectId, Guid initiatorId, string @ref, CancellationToken token = default)
    {
        return await context.Database.CreateExecutionStrategy().ExecuteAsync(async (ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            // 1. Verify project state
            var userProject = await context.UserProjects
                .Include(up => up.GitInfo)
                .FirstOrDefaultAsync(up => up.Id == userProjectId, ct)
                ?? throw new ServiceException(404, "User project not found.");

            ServiceException.ThrowIf(userProject.GitInfo is null, "Project has nothing submitted for review.");

            if (userProject.State is EntityObjectState.Awaiting or EntityObjectState.Inactive)
                throw new ServiceException(422, $"Project is currently {userProject.State.ToString().ToLower()} and cannot be reviewed.");

            if (userProject.State is EntityObjectState.Active)
                userProject.State = EntityObjectState.Awaiting;

            // 2. Resolve rubric
            var rubric = await context.Rubrics
                .Include(r => r.Variants)
                .Where(r => r.Enabled && (r.ProjectId == userProject.ProjectId || r.ProjectId == null))
                .OrderByDescending(r => r.ProjectId != null)
                .FirstOrDefaultAsync(ct)
                ?? throw new ServiceException(404, "No rubric available for this project.");

            var activeVariants = rubric.Variants.Where(v => v.Count > 0).ToList();
            ServiceException.ThrowIf(activeVariants.Count == 0, "Rubric has no active review kinds configured.");

            // 3. Preconditions & Membership Checks
            var member = await context.Members
                .FirstOrDefaultAsync(m => m.EntityType == MemberEntityType.UserProject && m.EntityId == userProjectId && m.UserId == initiatorId && m.LeftAt == null, ct);

            var isLeader = member?.Role is MemberRole.Leader;

            foreach (var variant in activeVariants)
            {
                if (variant.Kind is ReviewKinds.Self && member is null)
                    throw new ServiceException(422, "Only project members can request a self review.");

                if (variant.Kind is ReviewKinds.Peer or ReviewKinds.Async && (member is null || !isLeader))
                    throw new ServiceException(422, "Only team leaders can request a peer or async review.");
            }

            // 4. Business Rules
            var initiator = await context.Users.FirstOrDefaultAsync(u => u.Id == initiatorId, ct)
                ?? throw new ServiceException(404, "Initiator not found.");

            var ruleResult = await rules.CanRequestReviewAsync(rubric, initiator, userProject, ct);
            if (!ruleResult.IsSuccess)
                throw new ServiceException(string.Join("; ", ruleResult.Reasons));

            // 5. Generate Reviews
            var reviews = activeVariants
                .SelectMany(variant => Enumerable.Range(0, variant.Count).Select(_ => new Review
                {
                    RubricId = rubric.Id,
                    Kind = variant.Kind,
                    State = ReviewState.Pending,
                    UserProjectId = userProjectId,
                    Ref = @ref,
                    ReviewerId = variant.Kind is ReviewKinds.Self ? initiatorId : null,
                }))
                .ToList();

            _dbSet.AddRange(reviews);
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return reviews;
        }, token);
    }

    public async Task<Review> AssignReviewerAsync(Guid reviewId, Guid reviewerId, CancellationToken token = default)
    {
        var review = await _dbSet
            .Include(r => r.Rubric)
            .Include(r => r.UserProject)
            .FirstOrDefaultAsync(r => r.Id == reviewId, token)
            ?? throw new ServiceException(404, "Review not found.");

        ServiceException.ThrowIf(review.State is not ReviewState.Pending, "Review must be pending to assign a reviewer.");

        var reviewer = await context.Users.FirstOrDefaultAsync(u => u.Id == reviewerId, token)
            ?? throw new ServiceException(404, "Reviewer not found.");

        var membership = await context.Members
            .FirstOrDefaultAsync(m => m.EntityType == MemberEntityType.UserProject && m.EntityId == review.UserProjectId && m.UserId == reviewerId && m.LeftAt == null, token);

        switch (review.Kind)
        {
            case ReviewKinds.Self:
                ServiceException.ThrowIf(membership?.Role is not MemberRole.Leader, "Self reviews must be assigned to a project team leader.");
                break;
            case ReviewKinds.Peer:
            case ReviewKinds.Async:
                ServiceException.ThrowIf(membership is not null, $"{review.Kind} reviews must be assigned to someone outside the project team.");
                break;
            case ReviewKinds.Auto:
                throw new ServiceException(422, "Auto reviews cannot be manually assigned.");
        }

        var ruleResult = await rules.CanReviewAsync(review.Rubric, reviewer, review.UserProject, token);
        if (!ruleResult.IsSuccess)
            throw new ServiceException(403, string.Join("; ", ruleResult.Reasons));

        review.ReviewerId = reviewerId;
        await context.SaveChangesAsync(token);

        return review;
    }

    // ============================================================================
    // Public API (State Management)
    // ============================================================================

    public async Task<Review> StartReviewAsync(Guid reviewId, CancellationToken token = default)
    {
        var review = await _dbSet.FirstOrDefaultAsync(r => r.Id == reviewId, token)
            ?? throw new ServiceException(404, "Review not found.");

        ServiceException.ThrowIf(review.State is not ReviewState.Pending, "Review must be pending to start.");
        ServiceException.ThrowIf(review.ReviewerId is null && review.Kind is not ReviewKinds.Auto, "Review must have an assigned reviewer before starting.");

        review.State = ReviewState.InProgress;
        await context.SaveChangesAsync(token);

        return review;
    }

    public async Task<Review> CompleteReviewAsync(Guid reviewId, CancellationToken token = default)
    {
        var review = await _dbSet.FirstOrDefaultAsync(r => r.Id == reviewId, token)
            ?? throw new ServiceException(404, "Review not found.");

        ServiceException.ThrowIf(review.State is not ReviewState.InProgress, "Review must be in progress to complete.");

        review.State = ReviewState.Finished;
        await context.SaveChangesAsync(token);

        return review;
    }

    public async Task CancelReviewAsync(Guid reviewId, CancellationToken token = default)
    {
        var review = await _dbSet.FirstOrDefaultAsync(r => r.Id == reviewId, token)
            ?? throw new ServiceException(404, "Review not found.");

        ServiceException.ThrowIf(review.State is ReviewState.Finished, "Cannot cancel a finished review.");

        review.State = ReviewState.Cancelled;
        await context.SaveChangesAsync(token);
    }

    public async Task CancelAllReviewsAsync(Guid userProjectId, CancellationToken token = default)
    {
        var activeReviews = await _dbSet
            .Where(r => r.UserProjectId == userProjectId && r.State != ReviewState.Finished && r.State != ReviewState.Cancelled)
            .ToListAsync(token);

        foreach (var review in activeReviews)
        {
            review.State = ReviewState.Cancelled;
        }

        var userProject = await context.UserProjects.FirstOrDefaultAsync(up => up.Id == userProjectId, token);
        if (userProject?.State is EntityObjectState.Awaiting)
        {
            userProject.State = EntityObjectState.Active;
        }

        await context.SaveChangesAsync(token);
    }

    // ============================================================================
    // Public API (Queries)
    // ============================================================================

    public async Task<IEnumerable<Review>> GetPendingReviewsAsync(Guid userProjectId, CancellationToken token = default)
    {
        return await _dbSet
            .Where(r => r.UserProjectId == userProjectId && r.State == ReviewState.Pending)
            .Include(r => r.Rubric)
            .Include(r => r.Reviewer)
            .ToListAsync(token);
    }

    public async Task<IEnumerable<Review>> GetReviewerAssignmentsAsync(Guid reviewerId, CancellationToken token = default)
    {
        return await _dbSet
            .Where(r => r.ReviewerId == reviewerId && r.State != ReviewState.Finished)
            .Include(r => r.Rubric)
            .Include(r => r.UserProject)
            .ToListAsync(token);
    }

    public async Task<IEnumerable<Annotation>> GetAnnotationsAsync(Guid reviewId, string filePath, CancellationToken token = default)
    {
        return await context.Annotations
            .AsNoTracking()
            .Where(a => a.ReviewId == reviewId && a.FilePath == filePath)
            .ToListAsync(token);
    }

    public async Task<IEnumerable<Annotation>> SetAnnotationsAsync(Guid reviewId, Guid authorId, string filePath, IEnumerable<Annotation> annotations, CancellationToken token = default)
    {
        return await context.Database.CreateExecutionStrategy().ExecuteAsync(async (ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);
            await context.Annotations
                // .AsNoTracking()
                .Where(a => a.ReviewId == reviewId && a.FilePath == filePath)
                .ExecuteDeleteAsync(ct);

            context.Annotations.AddRange(annotations);
            await context.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
            return annotations;
        }, token);
    }
}