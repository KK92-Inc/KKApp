// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Quartz;
using Microsoft.EntityFrameworkCore;
using App.Backend.API.Jobs.Interfaces;
using App.Backend.Database;
using Wolverine;
using App.Backend.API.Bus.Messages.Trials;

// ============================================================================

namespace App.Backend.API.Jobs;

/// <summary>
/// Starts trials that are due and ends the ones that have run out.
///
/// Like the kickoff job this polls instead of scheduling a message ahead of time, so a trial that
/// was created or moved after the last run is still picked up, a date that moves in either direction
/// needs no cleanup, and downtime just means it runs on the next tick.
/// </summary>
[DisallowConcurrentExecution]
public class TrialJob(IMessageBus bus, DatabaseContext context, TimeProvider time) : IScheduledJob
{
    public static string? Schedule => "0 */5 * ? * *"; // Every 5 minutes

    public static string Identity => nameof(TrialJob);

    public async Task Execute(IJobExecutionContext job)
    {
        var now = time.GetUtcNow();
        var token = job.CancellationToken;

        var starting = await context.Trials
            .AsNoTracking()
            .Where(t => t.StartedAt == null && t.StartsAt <= now)
            .Select(t => t.Id)
            .ToListAsync(token);

        foreach (var id in starting)
            await bus.PublishAsync(new StartTrial(id));

        // Only trials that did start, one that never ran has to be started first.
        var ending = await context.Trials
            .AsNoTracking()
            .Where(t => t.StartedAt != null && t.EndedAt == null && t.EndsAt <= now)
            .Select(t => t.Id)
            .ToListAsync(token);

        foreach (var id in ending)
            await bus.PublishAsync(new EndTrial(id));
    }
}
