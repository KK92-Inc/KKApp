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
public class GoalHandler(
    IUserGoalService userGoalService,
    DatabaseContext context,
    IMessageBus bus,
    ILogger<GoalHandler> logger
)
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
        await bus.PublishAsync(new GoalCompleted(userGoal.User, userGoal));
    }
}