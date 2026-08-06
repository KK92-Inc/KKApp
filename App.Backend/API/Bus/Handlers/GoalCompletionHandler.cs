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
        var completedGoalIds = context.UserGoals
            .Where(ug => ug.UserId == userId && ug.State == EntityObjectState.Completed)
            .Select(ug => ug.GoalId);

        // Only cursi actively enrolled in, where this goal is actually part of
        // *this user's own frozen snapshot* - not just the live master track,
        // which may have moved on and no longer resembles the path they took.
        var eligibleUserCursi = await context.UserCursi
            .Where(uc => uc.UserId == userId && uc.State != EntityObjectState.Inactive)
            .Where(uc => context.UserCursusGoal.Any(ucg => ucg.UserCursusId == uc.Id && ucg.GoalId == goalId))
            .Select(uc => new { uc.Id, uc.CursusId })
            .ToListAsync(ct);

        if (eligibleUserCursi.Count == 0)
            return;

        var eligibleUserCursusIds = eligibleUserCursi.Select(uc => uc.Id).ToList();

        // Of those, keep only the ones where the user's own snapshot is fully satisfied:
        //   - Every required goal (ChoiceGroup == null) in their snapshot is completed
        //   - Every choice group in their snapshot has at least one completed goal
        var completedUserCursusIds = await context.UserCursusGoal
            .Where(ucg => eligibleUserCursusIds.Contains(ucg.UserCursusId))
            .GroupBy(ucg => ucg.UserCursusId)
            .Where(g =>
                g.Where(ucg => ucg.ChoiceGroup == null)
                    .All(ucg => completedGoalIds.Contains(ucg.GoalId)) &&
                g.Where(ucg => ucg.ChoiceGroup != null)
                    .GroupBy(ucg => ucg.ChoiceGroup)
                    .All(choiceGroup => choiceGroup.Any(ucg => completedGoalIds.Contains(ucg.GoalId))))
            .Select(g => g.Key)
            .ToHashSetAsync(ct);

        foreach (var uc in eligibleUserCursi.Where(uc => completedUserCursusIds.Contains(uc.Id)))
            await bus.PublishAsync(new CursusCompletionMessage(userId, uc.CursusId));
    }
}