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
public class UnFreezeUserHandler(
    DatabaseContext context,
    TimeProvider time,
    [FromKeyedServices("student")] IKeycloakService keycloak
)
{
    public async Task Handle(UnFreezeUserMessage message, CancellationToken token)
    {
        var now = time.GetUtcNow();
        var freeze = await context.Freezes
            .Where(f => f.UserId == message.UserId && f.StartsAt <= now && f.EndsAt <= now)
            .FirstOrDefaultAsync(token);

        // Basically there is no freeze anymore or it got invalidated before hand.
        if (freeze is null || freeze.InvalidatedAt is not null)
            return;

        // No-op if the account is already enabled.
        await keycloak.EnableUserAsync(message.UserId, token);
    }
}