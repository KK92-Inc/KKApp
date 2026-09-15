// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Bus.Handlers.Freeze;

using Wolverine.Attributes;
using App.Backend.Database;
using Keycloak.AuthServices.Sdk.Kiota.Admin;
using Microsoft.EntityFrameworkCore;
using App.Backend.API.Bus.Messages.Freeze;
using Wolverine;

// ============================================================================


[WolverineHandler]
public class UnFreezeUserHandler(
    DatabaseContext context,
    TimeProvider time,
    IMessageBus bus,
    IConfiguration configuration,
    [FromKeyedServices("student")] KeycloakAdminApiClient keycloak
)
{
    private readonly string realm = configuration["KeycloakStudent:realm"] ?? "student";

    public async Task Handle(UnFreezeUserMessage message, CancellationToken token)
    {
        var now = time.GetUtcNow();
        var freeze = await context.Freezes
            .Where(f => f.UserId == message.UserId && f.StartsAt <= now && f.EndsAt <= now)
            .FirstOrDefaultAsync(token);

        // Basically there is no freeze anymore or it got invalidated before hand.
        if (freeze is null || freeze.InvalidatedAt is not null) return;

        // Unlock the user.
        var studentRealm = keycloak.Admin.Realms[realm].Users[message.UserId.ToString()];
        var user = await studentRealm.GetAsync(null, token)
            ?? throw new InvalidOperationException($"{message.UserId} does not exist in keycloak");

        if (user.Enabled is false)
        {
            user.Enabled = true;
            await studentRealm.PutAsync(user, null, token);
        }
    }
}