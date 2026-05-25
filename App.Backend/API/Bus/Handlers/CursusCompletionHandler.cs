// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Bus.Handlers;

using Wolverine.Attributes;
using App.Backend.API.Bus.Messages;
using App.Backend.API.Notifications.Variants;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Enums;
using Wolverine;

// ============================================================================

[WolverineHandler]
public class CursusCompletionHandler(
    IMessageBus bus,
    IUserCursusService userCursusService,
    ILogger<CursusCompletionHandler> logger)
{
    public async Task Handle(CursusCompletionMessage message, CancellationToken ct)
    {
        var userCursus = await userCursusService.FindByUserAndCursusAsync(message.UserId, message.CursusId, ct);
        if (userCursus is null)
        {
            logger.LogError("UserCursus not found for UserId {UserId} and CursusId {CursusId}", message.UserId, message.CursusId);
            return;
        }

        if (userCursus.State is EntityObjectState.Completed)
            return;

        userCursus.State = EntityObjectState.Completed;
        await userCursusService.UpdateAsync(userCursus, ct);
        await bus.PublishAsync(new CursusCompletedNotification(
            userCursus.User.Id,
            userCursus.User.Login,
            userCursus.Id,
            userCursus.Cursus.Name
        ));
    }
}