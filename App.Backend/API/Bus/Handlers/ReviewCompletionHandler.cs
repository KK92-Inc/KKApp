// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Bus.Handlers;

using Resend;
using System.Text.Json;
using Wolverine.Attributes;
using Razor.Templating.Core;
using App.Backend.API.Notifications.Channels;
using App.Backend.API.Notifications;
using App.Backend.Core.Services.Interface;
using App.Backend.API.Notifications.Registers.Interface;
using App.Backend.API.Notifications.Variants;
using App.Backend.API.Bus.Messages;
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
        var review = await context.Reviews.FirstOrDefaultAsync(r => r.Id == message.ReviewId, ct);
        if (review is null)
        {
            logger.LogError("Review with ID {ReviewId} not found", message.ReviewId);
            return;
        }

        var userProject = await context.UserProjects
            .Include(up => up.Project)
            .Include(up => up.Reviews)
            .FirstOrDefaultAsync(up => up.Id == review.UserProjectId, ct);

        if (userProject is null)
        {
            logger.LogError("Review {ReviewId} has no associated UserProject", review.Id);
            return;
        }

        // Prefer project-specific rubric, fall back to wildcard
        var rubric = await context.Rubrics
            .Include(r => r.Variants)
            .Where(r => r.Enabled && (r.ProjectId == userProject.ProjectId || r.ProjectId == null))
            .OrderByDescending(r => r.ProjectId != null)
            .FirstOrDefaultAsync(ct);

        if (rubric is null)
        {
            logger.LogWarning("No rubric found for UserProject {UserProjectId}", userProject.Id);
            return;
        }

        // All required review kinds must be satisfied before we proceed
        var allComplete = rubric.Variants
            .Where(v => v.Count > 0)
            .All(v => userProject.Reviews.Count(r => r.Kind == v.Kind && r.State == ReviewState.Finished) >= v.Count);

        if (!allComplete)
        {
            logger.LogInformation("UserProject {UserProjectId} still has pending required reviews", userProject.Id);
            return;
        }

        userProject.State = EntityObjectState.Completed;
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

        await context.SaveChangesAsync(ct);
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