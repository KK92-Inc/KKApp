// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Bus.Handlers;

using Wolverine.Attributes;
using App.Backend.Database;
using Wolverine;
using JasperFx.Events.Documents;
using Keycloak.AuthServices.Sdk.Kiota.Admin;
using App.Backend.API.Bus.Messages.Kickoff;

// ============================================================================

/// <summary>
/// Handles a starting kickoff.
/// 
/// Kickoffs essentially just activate user accounts and promotes them to
/// actual students into a campus. Unless they were already students then
/// nothing changes.
/// </summary>
[WolverineHandler]
public class KickoffHandler(DatabaseContext context, IMessageBus bus, TimeProvider time)
{
    public async Task<IEnumerable<object>> Handle(StartKickoff message, CancellationToken ct)
    {
        return [];
        // var kickoff = await context.Kickoffs.FirstOrDefaultAsync(k => k.Id == message.KickoffId, ct);
        // if (kickoff is null) return [];

        // var now = time.GetUtcNow();
        // if (kickoff.StartsAt > now)
        // {
        //     await bus.ScheduleAsync(message, kickoff.StartsAt);
        //     return [];
        // }

        // var pending = await context.UserKickoff
        //     .Where(uk => uk.KickoffId == kickoff.Id)
        //     .Select(uk => new { uk.UserId, uk.KickoffId })
        //     .ToListAsync(ct);

        // return pending.Select(uk => new PromoteApplicant(uk.UserId, uk.KickoffId));
    }
}