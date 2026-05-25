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
public class ReviewHandler(
    INotificationService notificationService,
    DatabaseContext context,
    IMessageBus bus,
    ILogger<ReviewHandler> logger
)
{
    public async Task Handle(ReviewCompleted message, CancellationToken ct)
    {
        var review = await context.Reviews.FirstOrDefaultAsync(r => r.Id == message.ReviewId, ct);
        if (review is null)
        {
            logger.LogError("Review with ID {ReviewId} not found", message.ReviewId);
            return;
        }

        var userProject = await context.UserProjects
            .Include(up => up.Project)
            .FirstOrDefaultAsync(up => up.Id == review.UserProjectId, ct);

        if (userProject is null)
        {
            logger.LogError("Review {ReviewId} has no associated UserProject", review.Id);
            return;
        }

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
        // Subquery: UserProject IDs this user has completed (via Members)
        var completedUserProjects = context.Members
            .Where(m =>
                m.EntityType == MemberEntityType.UserProject &&
                m.UserId == userId &&
                m.LeftAt == null)
            .Join(
                context.UserProjects.Where(up => up.State == EntityObjectState.Completed),
                m => m.EntityId,
                up => up.Id,
                (m, up) => up.ProjectId);   // <-- IQueryable, not materialized

        // Single query:
        // - Goals that contain this project
        // - User is subscribed to them
        // - Every project in the goal is completed by the user
        var completedGoalIds = await context.GoalProject
            .Where(gp =>
                context.GoalProject.Any(inner => inner.ProjectId == projectId && inner.GoalId == gp.GoalId) &&
                context.UserGoals.Any(ug => ug.UserId == userId && ug.GoalId == gp.GoalId))
            .GroupBy(gp => gp.GoalId)
            .Where(g => g.All(gp => completedUserProjects.Contains(gp.ProjectId)))
            .Select(g => g.Key)
            .ToListAsync(ct);               // only GUIDs, nothing more

        // TODO: Publish notification...
        foreach (var goalId in completedGoalIds)
            await bus.PublishAsync(new GoalCompletionMessage(userId, goalId));
    }
}