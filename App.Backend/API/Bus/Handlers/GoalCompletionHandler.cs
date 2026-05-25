// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Bus.Handlers;

using Wolverine;
using Wolverine.Attributes;
using Microsoft.EntityFrameworkCore;
using App.Backend.API.Bus.Messages;
using App.Backend.API.Notifications.Variants;
using App.Backend.Core.Services.Interface;
using App.Backend.Database;
using App.Backend.Domain.Enums;

// ============================================================================

[WolverineHandler]
public class GoalCompletionHandler(
    IUserGoalService userGoalService,
    DatabaseContext context,
    IMessageBus bus,
    ILogger<GoalCompletionHandler> logger)
{
    public async Task Handle(GoalCompletionMessage message, CancellationToken ct)
    {
        var userGoal = await userGoalService.FindByUserAndGoalAsync(message.UserId, message.GoalId, ct);
        if (userGoal is null)
        {
            logger.LogError("UserGoal not found for UserId {UserId} and GoalId {GoalId}", message.UserId, message.GoalId);
            return;
        }

        if (userGoal.State is EntityObjectState.Completed)
            return;

        userGoal.State = EntityObjectState.Completed;
        await userGoalService.UpdateAsync(userGoal, ct);

        await bus.PublishAsync(new GoalCompletedNotification(
            userGoal.User.Id,
            userGoal.User.Login,
            userGoal.Id,
            userGoal.Goal.Name
        ));

        await CheckCursusProgressionAsync(message.UserId, message.GoalId, ct);
    }

    // -------------------------------------------------------------------------

    private async Task CheckCursusProgressionAsync(Guid userId, Guid goalId, CancellationToken ct)
    {
        // All goals this user has completed (including the one just marked above)
        var completedGoalIds = context.UserGoals
            .Where(ug => ug.UserId == userId && ug.State == EntityObjectState.Completed)
            .Select(ug => ug.GoalId);

        // Cursi that contain the just-completed goal AND the user is enrolled in
        var eligibleCursusIds = context.CursusGoal
            .Where(cg => cg.GoalId == goalId)
            .Select(cg => cg.CursusId)
            .Where(cursusId => context.UserCursi.Any(uc => uc.UserId == userId && uc.CursusId == cursusId));

        // Of those, keep only cursi where the full track is satisfied:
        //   - Every required goal (ChoiceGroup == null) is completed
        //   - Every choice group has at least one completed goal
        var completedCursusIds = await context.CursusGoal
            .Where(cg => eligibleCursusIds.Contains(cg.CursusId))
            .GroupBy(cg => cg.CursusId)
            .Where(g =>
                g.Where(cg => cg.ChoiceGroup == null)
                    .All(cg => completedGoalIds.Contains(cg.GoalId)) &&
                g.Where(cg => cg.ChoiceGroup != null)
                    .GroupBy(cg => cg.ChoiceGroup)
                    .All(choiceGroup => choiceGroup.Any(cg => completedGoalIds.Contains(cg.GoalId))))
            .Select(g => g.Key)
            .ToListAsync(ct);

        foreach (var cursusId in completedCursusIds)
            await bus.PublishAsync(new CursusCompletionMessage(userId, cursusId));
    }
}