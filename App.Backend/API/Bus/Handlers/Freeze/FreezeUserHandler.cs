// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Bus.Handlers.Freeze;

using Wolverine.Attributes;
using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using Microsoft.EntityFrameworkCore;
using App.Backend.API.Bus.Messages.Freeze;
using Wolverine;

// ============================================================================


[WolverineHandler]
public class FreezeUserHandler(
    DatabaseContext context,
    TimeProvider time,
    IMessageBus bus,
    [FromKeyedServices("student")] IKeycloakService keycloak
)
{
    public async Task Handle(FreezeUserMessage message, CancellationToken token)
    {
        var now = time.GetUtcNow();
        var freeze = await context.Freezes
            .Where(f => f.UserId == message.UserId && f.StartsAt <= now && f.EndsAt > now)
            .FirstOrDefaultAsync(token);

        // No active freeze found (cancelled, expired, or never started).
        if (freeze is null) return;

        // No-op if the account is already disabled.
        // Schedule the unfreeze on that date
        await keycloak.DisableUserAsync(freeze.UserId, token);
        await bus.ScheduleAsync(new UnFreezeUserMessage(freeze.UserId), freeze.EndsAt);
    }
}