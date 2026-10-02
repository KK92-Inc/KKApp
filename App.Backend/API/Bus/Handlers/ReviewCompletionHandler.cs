// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Bus.Handlers;

using System.Text.Json;
using Wolverine.Attributes;
using App.Backend.API.Notifications.Channels;
using App.Backend.API.Notifications;
using App.Backend.Core.Services.Interface;
using App.Backend.API.Notifications.Registers.Interface;
using App.Backend.API.Notifications.Variants;
using App.Backend.API.Bus.Messages;
using App.Backend.Core.Engines.Reviews;
using App.Backend.Database;
using Microsoft.EntityFrameworkCore;
using App.Backend.Domain.Enums;
using Wolverine;

// ============================================================================


[WolverineHandler]
public class ReviewCompletionHandler(DatabaseContext context, IMessageBus bus, ILogger<ReviewCompletionHandler> logger)
{
    public async Task Handle(ReviewCompletionMessage message, CancellationToken ct)
    {
        var review = await context.Reviews.AsNoTracking().FirstOrDefaultAsync(r => r.Id == message.ReviewId, ct);
        if (review is null)
        {
            logger.LogError("Review with ID {ReviewId} not found", message.ReviewId);
            return;
        }

        // Advisory feedback never influences the outcome of the project.
        if (review.RoundId is null)
        {
            logger.LogDebug("Review {ReviewId} is advisory, nothing to resolve", review.Id);
            return;
        }

        var round = await context.ReviewRounds
            .AsNoTracking()
            .Include(r => r.Reviews)
            .FirstOrDefaultAsync(r => r.Id == review.RoundId, ct);

        if (round is null || round.State is not ReviewRoundState.Open)
        {
            logger.LogInformation("Round {RoundId} is no longer open, ignoring review {ReviewId}", review.RoundId, review.Id);
            return;
        }

        var outcome = ReviewRoundResolver.Resolve(round.Reviews);
        if (outcome is ReviewRoundOutcome.Pending)
        {
            logger.LogInformation("Round {RoundId} still has pending reviews", round.Id);
            return;
        }

        // Close the round atomically so concurrent completions can't resolve it twice
        // (which would e.g. publish goal completions twice).
        var next = outcome is ReviewRoundOutcome.Passed ? ReviewRoundState.Passed : ReviewRoundState.Failed;
        var now = DateTimeOffset.UtcNow;
        var closed = await context.ReviewRounds
            .Where(r => r.Id == round.Id && r.State == ReviewRoundState.Open)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.State, next)
                .SetProperty(r => r.ClosedAt, (DateTimeOffset?)now)
                .SetProperty(r => r.UpdatedAt, now), ct);

        if (closed is 0)
            return;

        var userProject = await context.UserProjects.FirstOrDefaultAsync(up => up.Id == round.UserProjectId, ct);
        if (userProject is null)
        {
            logger.LogError("Round {RoundId} has no associated UserProject", round.Id);
            return;
        }

        if (outcome is ReviewRoundOutcome.Failed)
        {
            // Nobody should keep working on a round that is already decided.
            await context.Reviews
                .Where(r => r.RoundId == round.Id && (r.State == ReviewState.Pending || r.State == ReviewState.InProgress))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.State, ReviewState.Cancelled)
                    .SetProperty(r => r.UpdatedAt, now), ct);

            // Unlock the session so the team can fix things and request a new round.
            if (userProject.State is EntityObjectState.Awaiting)
            {
                userProject.State = EntityObjectState.Active;
                await context.SaveChangesAsync(ct);
            }

            logger.LogInformation("Round {RoundId} of UserProject {UserProjectId} failed", round.Id, userProject.Id);
            return;
        }

        if (userProject.State is not (EntityObjectState.Awaiting or EntityObjectState.Active))
        {
            logger.LogWarning("UserProject {UserProjectId} is {State}, not completing it", userProject.Id, userProject.State);
            return;
        }

        userProject.State = EntityObjectState.Completed;
        await context.SaveChangesAsync(ct);

        var memberIds = await context.Members
            .Where(m =>
                m.EntityType == MemberEntityType.UserProject &&
                m.EntityId == userProject.Id &&
                m.LeftAt == null)
            .Select(m => m.UserId)
            .ToListAsync(ct);

        if (memberIds.Count is 0)
        {
            logger.LogWarning("UserProject {UserProjectId} has no active members", userProject.Id);
            return;
        }

        foreach (var userId in memberIds)
            await CheckGoalProgressionAsync(userId, userProject.ProjectId, ct);
    }

    private async Task CheckGoalProgressionAsync(Guid userId, Guid projectId, CancellationToken ct)
    {
        var completedUserProjects = context.Members
            .Where(m =>
                m.EntityType == MemberEntityType.UserProject &&
                m.UserId == userId &&
                m.LeftAt == null)
            .Join(
                context.UserProjects.Where(up => up.State == EntityObjectState.Completed),
                m => m.EntityId,
                up => up.Id,
                (m, up) => up.ProjectId);

        var completedGoalIds = await context.GoalProject
            .Where(gp =>
                context.GoalProject.Any(inner => inner.ProjectId == projectId && inner.GoalId == gp.GoalId) &&
                context.UserGoals.Any(ug => ug.UserId == userId && ug.GoalId == gp.GoalId))
            .GroupBy(gp => gp.GoalId)
            .Where(g => g.All(gp => completedUserProjects.Contains(gp.ProjectId)))
            .Select(g => g.Key)
            .ToListAsync(ct);

        foreach (var goalId in completedGoalIds)
            await bus.PublishAsync(new GoalCompletionMessage(userId, goalId));
    }
}