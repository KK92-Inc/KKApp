// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Quartz;
using Microsoft.EntityFrameworkCore;
using App.Backend.API.Jobs.Interfaces;
using App.Backend.Database;
using Wolverine;
using App.Backend.API.Bus.Messages.Kickoff;

// ============================================================================

namespace App.Backend.API.Jobs;

/// <summary>
/// Starts kickoffs that are due.
///
/// This polls instead of scheduling a message at the start time ahead of time, so a kickoff
/// that was created or moved after the last run is still picked up, a start time that moves in
/// either direction needs no cleanup, and downtime just means it runs on the next tick.
///
/// It only announces which kickoffs are due, the kickoff handler does the
/// work and is safe to run more than once for the same kickoff.
/// </summary>
[DisallowConcurrentExecution]
public class KickoffJob(IMessageBus bus, DatabaseContext context, TimeProvider time) : IScheduledJob
{
    public static string? Schedule => "0 */5 * ? * *"; // Every 5 minutes

    public static string Identity => nameof(KickoffJob);

    public async Task Execute(IJobExecutionContext job)
    {
        var now = time.GetUtcNow();
        var due = await context.Kickoffs
            .AsNoTracking()
            .Where(k => k.StartedAt == null && k.StartsAt <= now)
            .Select(k => k.Id)
            .ToListAsync(job.CancellationToken);

        foreach (var id in due)
            await bus.PublishAsync(new StartKickoff(id));
    }
}
