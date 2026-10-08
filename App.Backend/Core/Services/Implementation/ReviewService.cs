// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;
using App.Backend.Core.Query;

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class ReviewService(
    DatabaseContext ctx,
    TimeProvider time,
    IRuleService rules,
    IGitService git
) : BaseService<Review>(ctx), IReviewService
{
    private readonly DatabaseContext context = ctx;

    public override async Task<PaginatedList<Review>> GetAllAsync(ISorting sorting, IPagination pagination, CancellationToken token = default, params Expression<Func<Review, bool>>?[] filters)
    {
        return await filters
            .Where(f => f is not null)
            .Aggregate(_dbSet.AsQueryable(), (c, filter) => c.Where(filter!))
            .Sort(sorting)
            .Include(r => r.Rubric)
            .Include(r => r.UserProject)
            .ThenInclude(up => up.Project)
            .ThenInclude(p => p.Workspace)
            .Include(r => r.Reviewer)
            .PaginateAsync(pagination, token);
    }

    public async Task<ReviewRound> PullReviewAsync(Guid userProjectId, Guid initiatorId, CancellationToken token = default)
    {
        var up = await context.UserProjects
            .Include(up => up.GitInfo)
            .FirstOrDefaultAsync(up => up.Id == userProjectId, token)
            ?? throw new ServiceException(404, "User project not found.");

        ServiceException.ThrowIf(up.GitInfo is null, "Project has nothing submitted for review.");
        ServiceException.ThrowIf(up.State is EntityObjectState.Completed, "Project is already completed.");
        ServiceException.ThrowIf(up.State is EntityObjectState.Inactive, "Project is currently inactive and cannot be reviewed.");

        var activeRound = await context.ReviewRounds.AnyAsync(
            r => r.UserProjectId == userProjectId &&
            r.State == ReviewRoundState.Open,
        token);

        ServiceException.ThrowIf(activeRound, 409, "This session already has an open review round.");

        var @ref = await git.GetDefaultBranchAsync(up.GitInfo.Owner, up.GitInfo.Name, token);
        ServiceException.ThrowIf(@ref is null, "Session repository has no default branch.");

        // Pin the exact commit: the branch will move on, the round must keep pointing at what was submitted.
        var sha = await git.ResolveShaAsync(up.GitInfo.Owner, up.GitInfo.Name, @ref, token);
        ServiceException.ThrowIf(sha is null, "Could not resolve the submitted commit.");

        var rubric = await GetRubricForProjectAsync(up.ProjectId, token);
        var variants = rubric.Variants.Where(v => v.Count > 0).ToList();
        ServiceException.ThrowIf(variants.Count == 0, "Rubric has no active review kinds configured. Please report to staff.");

        var member = await context.Members
            .Include(m => m.User)
            .FirstOrDefaultAsync(
                m => m.EntityType == MemberEntityType.UserProject &&
                m.EntityId == userProjectId &&
                m.UserId == initiatorId &&
                m.LeftAt == null,
            token);

        ServiceException.ThrowIf(member is null, "Must be a part of this session to request a review round.");
        ServiceException.ThrowIf(member.Role is not MemberRole.Leader, "Only the project session leader can request review rounds.");

        // TODO: Do we need to invoke the rule engine ... ? I don't think so.

        // NOTE(W2): Needs to be like this else we get a unhandled exception
        // in regards to the nullable attempt.
        var maxAttempt = await context.ReviewRounds
            .Where(r => r.UserProjectId == userProjectId)
            .MaxAsync(r => (int?)r.Attempt, token) ?? 0;

        return await context.Database.CreateExecutionStrategy().ExecuteAsync(async (ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            var round = new ReviewRound
            {
                UserProjectId = userProjectId,
                Attempt = maxAttempt + 1,
                RubricId = rubric.Id,
                Ref = @ref,
                Sha = sha,
                RequestedById = initiatorId,
            };

            var slots = variants.SelectMany(v => Enumerable.Range(0, v.Count).Select(_ => new Review
            {
                RoundId = round.Id, // Tie this review to the round.
                RubricId = rubric.Id,
                Kind = v.Kind,
                State = ReviewState.Pending,
                UserProjectId = userProjectId,
                Ref = @ref,
                Sha = sha,
                ReviewerId = v.Kind is ReviewKinds.Self ? initiatorId : null, // We can already determine the evaluator.
            }));

            up.State = EntityObjectState.Awaiting;
            var result = context.ReviewRounds.Add(round);
            _dbSet.AddRange(slots);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return result.Entity;
        }, token);
    }

    public async Task<Review> PushReviewAsync(Guid userProjectId, ReviewKinds kind, Guid reviewerId, CancellationToken token = default)
    {
        switch (kind)
        {
            case ReviewKinds.Auto:
                throw new ServiceException(501, "Auto reviews are not implemented yet.");
            case ReviewKinds.Peer:
                throw new ServiceException("Peer reviews are live sessions: pick up a free slot of an open evaluation round instead.");
        }

        // Schedule is ensured to be valid for Async (see PostPushReviewRequestDTO), Self ignores it.
        var up = await context.UserProjects
            .Include(up => up.GitInfo)
            .FirstOrDefaultAsync(up => up.Id == userProjectId, token)
            ?? throw new ServiceException(404, "User project not found.");

        ServiceException.ThrowIf(up.GitInfo is null, "Project has nothing submitted for review.");

        var reviewer = await context.Users.FirstOrDefaultAsync(u => u.Id == reviewerId, token)
            ?? throw new ServiceException(404, "Reviewer not found.");

        // Most relevant membership: an active one wins, otherwise the one that ended last.
        var now = time.GetUtcNow();
        var membership = await context.Members
            .Where(m => m.EntityType == MemberEntityType.UserProject &&
                        m.EntityId == userProjectId &&
                        m.UserId == reviewerId)
            .Select(m => new
            {
                IsActive = m.LeftAt == null,
                m.Role,
                m.LeftAt
            })
            .OrderByDescending(m => m.IsActive)
            .ThenByDescending(m => m.LeftAt)
            .FirstOrDefaultAsync(token);

        if (kind is ReviewKinds.Self)
        {
            // You evaluate your own work, so this is the exact opposite of Async.
            ServiceException.ThrowIf(
                membership is not { IsActive: true, Role: MemberRole.Leader },
                "Only the team leader can self review the project."
            );
        }
        else
        {
            // NOTE(W2): One hard rule: you can't evaluate as a member.
            // Beyond that, anyone can give a review on anyone's project, at any time.
            if (membership is not null)
            {
                ServiceException.ThrowIf(
                    membership.IsActive,
                    "You cannot evaluate a project you are currently a member of."
                );

                ServiceException.ThrowIf( // TODO: Configurable
                    membership.LeftAt > now.AddDays(-14),
                    "You must wait at least 2 weeks after leaving a project before you can evaluate it."
                );
            }
        }

        return await context.Database.CreateExecutionStrategy().ExecuteAsync(async (ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            // Read the round inside the transaction so it can't close underneath us.
            var round = await context.ReviewRounds
                .Include(r => r.Rubric)
                .ThenInclude(r => r.Variants)
                .FirstOrDefaultAsync(
                    r => r.UserProjectId == userProjectId &&
                    r.State == ReviewRoundState.Open,
                ct);

            // The round's pinned rubric wins while it is open, otherwise the project's current one.
            var rubric = round?.Rubric ?? await GetRubricForProjectAsync(up.ProjectId, ct);
            ServiceException.ThrowIf(
                !rubric.Variants.Any(v => v.Kind == kind && v.Count > 0),
                $"Rubric does not support {kind} reviews."
            );

            // Reviewer eligibility only makes sense when evaluating somebody else's work.
            if (kind is not ReviewKinds.Self)
            {
                var ruleResult = await rules.CanReviewAsync(rubric, reviewer, up, ct);
                ServiceException.ThrowIf(!ruleResult.IsSuccess, 403, string.Join("; ", ruleResult.Reasons));
            }

            // There is an active round: try to claim a slot.
            if (round is not null)
            {
                var alreadyHolds = await _dbSet.AnyAsync(r =>
                    r.RoundId == round.Id &&
                    r.ReviewerId == reviewerId &&
                    r.State != ReviewState.Cancelled,
                ct);

                // NOTE: A leader's self slot is pre-assigned when the round is requested, so for Self
                // this means "go start the slot you already have" rather than "claim another one".
                ServiceException.ThrowIf(
                    alreadyHolds,
                    kind is ReviewKinds.Self
                        ? "Your self review for this evaluation round is already assigned to you."
                        : "You already hold a review slot in this evaluation round."
                );

                var slots = await _dbSet
                    .AsNoTracking()
                    .Where(
                        r => r.RoundId == round.Id &&
                        r.Kind == kind &&
                        r.ReviewerId == null &&
                        r.State == ReviewState.Pending
                    )
                    .OrderBy(r => r.CreatedAt)
                    .Select(r => r.Id)
                    .ToListAsync(ct);

                foreach (var slot in slots)
                {
                    if (!await TryClaimSlotAsync(slot, reviewerId, ct))
                        continue;

                    await transaction.CommitAsync(ct);
                    return await _dbSet
                        .Include(r => r.Rubric)
                        .Include(r => r.UserProject)
                        .Include(r => r.Reviewer)
                        .FirstAsync(r => r.Id == slot, ct);
                }

                // There WERE free slots but somebody beat us to all of them. Say so rather than silently
                // downgrading to feedback that doesn't count, the reviewer would assume it does.
                ServiceException.ThrowIf(
                    slots.Count > 0,
                    409, "That review slot was just taken by someone else, try again."
                );
            }

            // Advisory review: feedback only. It has no round so it never counts for or against
            // the project, no matter the project's state (even completed, even years later).
            var @ref = await git.GetDefaultBranchAsync(up.GitInfo!.Owner, up.GitInfo.Name, ct);
            ServiceException.ThrowIf(@ref is null, "Session repository has no default branch.");

            var sha = await git.ResolveShaAsync(up.GitInfo.Owner, up.GitInfo.Name, @ref, ct);
            ServiceException.ThrowIf(sha is null, "Could not resolve the reviewed commit.");

            var review = new Review
            {
                RoundId = null,
                RubricId = rubric.Id,
                Kind = kind,
                State = ReviewState.Pending,
                UserProjectId = userProjectId,
                Ref = @ref,
                Sha = sha,
                ReviewerId = reviewerId,
                ClaimedAt = now,
            };

            var result = _dbSet.Add(review);
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return result.Entity;
        }, token);
    }

    public async Task<ReviewRound> CancelRoundAsync(Guid roundId, CancellationToken token = default)
    {
        var round = await context.ReviewRounds
            .Include(r => r.Reviews)
            .FirstOrDefaultAsync(r => r.Id == roundId, token)
            ?? throw new ServiceException(404, "Review round not found.");

        ServiceException.ThrowIf(round.State is not ReviewRoundState.Open, "Only an open round can be cancelled.");

        round.State = ReviewRoundState.Cancelled;
        round.ClosedAt = time.GetUtcNow();

        foreach (var review in round.Reviews.Where(r => r.State is ReviewState.Pending or ReviewState.InProgress))
            review.State = ReviewState.Cancelled;

        return await context.Database.CreateExecutionStrategy().ExecuteAsync(async (ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            var existing = await context.UserProjects.FirstOrDefaultAsync(up => up.Id == round.UserProjectId, token);
            ServiceException.ThrowIf(existing is null, 404, "Session does not exist anymore");

            if (existing.State is EntityObjectState.Awaiting)
                existing.State = EntityObjectState.Active;

            // Lock the session for at least 1 hour, to prevent constant
            // Request and cancellations.
            existing.UnlocksAt = time.GetUtcNow().Add(TimeSpan.FromHours(1));
            context.UserProjects.Update(existing);

            await context.UserProjectTransactions.AddAsync(new()
            {
                UserProjectId = existing.Id,
                Type = UserProjectTransactionVariant.StateChangedToActive,
            }, ct);

            await git.LockAsync(existing.ProjectId.ToString(), existing.Id.ToString(), ct);
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return round;
        }, token);
    }

    public async Task<ReviewRound?> FindRoundByIdAsync(Guid roundId, CancellationToken token = default)
    {
        return await context.ReviewRounds
            .Include(r => r.Reviews)
            .FirstOrDefaultAsync(r => r.Id == roundId, token);
    }

    public async Task<IEnumerable<ReviewRound>> GetRoundsAsync(Guid userProjectId, CancellationToken token = default)
    {
        return await context.ReviewRounds
            .AsNoTracking()
            .Where(r => r.UserProjectId == userProjectId)
            .Include(r => r.Rubric)
            .ThenInclude(r => r.GitInfo)
            .Include(r => r.Reviews)
            .OrderByDescending(r => r.Attempt)
            .ToListAsync(token);
    }

    public async Task<Review> StartReviewAsync(Guid reviewId, CancellationToken token = default)
    {
        var review = await _dbSet.Include(r => r.Round).FirstOrDefaultAsync(r => r.Id == reviewId, token)
            ?? throw new ServiceException(404, "Review not found.");

        ServiceException.ThrowIf(review.State is not ReviewState.Pending, "Review must be pending to start.");
        ServiceException.ThrowIf(review.ReviewerId is null && review.Kind is not ReviewKinds.Auto, "Review must have an assigned reviewer before starting.");
        ServiceException.ThrowIf(review.Round is { State: not ReviewRoundState.Open }, "The evaluation round of this review is closed.");

        review.State = ReviewState.InProgress;
        review.StartedAt = time.GetUtcNow();
        await context.SaveChangesAsync(token);
        return review;
    }

    public async Task<Review> StopReviewAsync(Guid reviewId, CancellationToken token = default)
    {
        var review = await _dbSet.Include(r => r.Round).FirstOrDefaultAsync(r => r.Id == reviewId, token)
            ?? throw new ServiceException(404, "Review not found.");

        ServiceException.ThrowIf(review.State is ReviewState.Finished, "Cannot cancel a finished review.");
        ServiceException.ThrowIf(review.State is ReviewState.Cancelled, "Review is already cancelled.");
        if (review.RoundId is not null)
        {
            ServiceException.ThrowIf(review.Round!.State is not ReviewRoundState.Open, "The evaluation round of this review is closed.");
            ServiceException.ThrowIf(review.Kind is ReviewKinds.Auto, "Auto reviews can't be cancelled individually, cancel the round instead.");

            // The round still needs this slot filled, so release it for somebody else
            // rather than cancelling it. Cancelling would leave the round unresolvable.
            review.ReviewerId = null;
            review.State = ReviewState.Pending;
            review.ClaimedAt = null;
            review.StartedAt = null;
        }
        else
        {
            // Boo hoo who cares.
            review.State = ReviewState.Cancelled;
        }

        await context.SaveChangesAsync(token);
        return review;
    }

    public async Task<Review> CompleteReviewAsync(Guid reviewId, bool passed, IEnumerable<Annotation> annotations, CancellationToken token = default)
    {
        var review = await _dbSet.Include(r => r.Round).FirstOrDefaultAsync(r => r.Id == reviewId, token)
            ?? throw new ServiceException(404, "Review not found.");

        ServiceException.ThrowIf(review.State is not ReviewState.InProgress, "Review must be in progress to complete.");
        if (review.RoundId is not null)
            ServiceException.ThrowIf(review.Round!.State is not ReviewRoundState.Open, "The evaluation round of this review is closed.");

        return await context.Database.CreateExecutionStrategy().ExecuteAsync(async (ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            // Mark review as complete
            review.Passed = passed;
            review.State = ReviewState.Finished;
            review.FinishedAt = time.GetUtcNow();

            // Write any annotations the reviewer has given
            await context.AddRangeAsync(annotations, ct);
            await context.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
            return review;
        }, token);
    }

    public async Task<Review> AssignReviewerAsync(Guid reviewId, Guid reviewerId, CancellationToken token = default)
    {
        var review = await _dbSet
            .Include(r => r.Rubric)
            .Include(r => r.UserProject)
            .Include(r => r.Round)
            .FirstOrDefaultAsync(r => r.Id == reviewId, token);

        ServiceException.ThrowIf(review is null, 404, "Review not found.");
        ServiceException.ThrowIf(review.State is not ReviewState.Pending, "Review must be pending to assign a reviewer.");
        ServiceException.ThrowIf(review.RoundId is null, "Only review slots of an evaluation round can be assigned.");
        ServiceException.ThrowIf(review.Round!.State is not ReviewRoundState.Open, "The evaluation round of this review is closed.");

        // E.g: Self review), basically already assigned.
        if (review.ReviewerId == reviewerId)
            return review;

        ServiceException.ThrowIf(review.ReviewerId is not null, 409, "This review has already been claimed.");

        var membership = await context.Members
            .Include(m => m.User)
            .FirstOrDefaultAsync(
                m => m.EntityType == MemberEntityType.UserProject &&
                m.EntityId == review.UserProjectId &&
                m.UserId == reviewerId &&
                m.LeftAt == null,
            token);

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

        var reviewer = await context.Users.FirstOrDefaultAsync(u => u.Id == reviewerId, token)
            ?? throw new ServiceException(404, "Reviewer / user not found.");

        // Does reviewer qualify to review ?
        var result = await rules.CanReviewAsync(review.Rubric, reviewer, review.UserProject, token);
        ServiceException.ThrowIf(!result.IsSuccess, 403, string.Join("; ", result.Reasons));

        if (!await TryClaimSlotAsync(review.Id, reviewerId, token))
            throw new ServiceException(409, "This review was just claimed by someone else, or you already hold a slot in this round.");

        return await _dbSet
            .AsNoTracking()
            .Include(r => r.Rubric)
            .Include(r => r.UserProject)
            .Include(r => r.Reviewer)
            .FirstAsync(r => r.Id == reviewId,
        token);
    }

    // ============================================================================

    /// <summary>
    /// Prefer the project-specific rubric, fall back to the wildcard one.
    /// </summary>
    private async Task<Rubric> GetRubricForProjectAsync(Guid projectId, CancellationToken ct)
    {
        return await context.Rubrics
            .Include(r => r.Variants)
            .Where(r => r.Enabled && (r.ProjectId == projectId || r.ProjectId == null))
            .OrderByDescending(r => r.ProjectId != null)
            .FirstOrDefaultAsync(ct)
            ?? throw new ServiceException(404, "No rubric available for this project.");
    }

    /// <summary>
    /// First come, first served: atomically attaches the reviewer to a slot, only if it is
    /// still unclaimed and pending. Also refuses when the reviewer already holds another
    /// slot in the same round, so one person can't satisfy two required reviews.
    /// </summary>
    /// <returns>True if this reviewer got the slot.</returns>
    private async Task<bool> TryClaimSlotAsync(
        Guid reviewId,
        Guid reviewerId,
        CancellationToken ct
    )
    {
        var now = DateTimeOffset.UtcNow;
        var rows = await context.Reviews
            .Where(r => r.Id == reviewId
                && r.ReviewerId == null
                && r.State == ReviewState.Pending
                && !context.Reviews.Any(o => o.RoundId == r.RoundId
                    && o.ReviewerId == reviewerId
                    && o.State != ReviewState.Cancelled))
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.ReviewerId, (Guid?)reviewerId)
                .SetProperty(r => r.ClaimedAt, (DateTimeOffset?)now)
                .SetProperty(r => r.UpdatedAt, now), ct);

        return rows == 1;
    }
}