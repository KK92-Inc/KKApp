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

[DisallowConcurrentExecution]
public class KickoffJob(IMessageBus bus, DatabaseContext context, TimeProvider time) : IScheduledJob
{
    public static string? Schedule => "0 0 * * *"; // Every night at 00:00 UTC

    public static string Identity => nameof(KickoffJob);

    public async Task Execute(IJobExecutionContext job)
    {
        var startOfDay = time.GetUtcNow().Date;
        var endOfDay = startOfDay.AddDays(1);

        var pending = await context.Kickoffs
            .AsNoTracking()
            .Where(k => k.StartsAt >= startOfDay && k.StartsAt < endOfDay)
            .Select(k => new { k.Id, k.StartsAt })
            .ToListAsync(job.CancellationToken);

        foreach (var kickoff in pending)
        {
            await bus.ScheduleAsync(new StartKickoff(kickoff.Id), kickoff.StartsAt);
        }
    }
}