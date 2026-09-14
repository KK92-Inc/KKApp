// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Quartz;
using Microsoft.EntityFrameworkCore;
using App.Backend.API.Jobs.Interfaces;
using App.Backend.Database;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.API.Jobs;

/// <summary>
/// Periodically sweeps events to auto-reject under-threshold Pending events
/// nearing their start time, and to mark ended Accepted events as Completed.
/// </summary>
[DisallowConcurrentExecution]
public class EventStateJob(ILogger<EventStateJob> logger, DatabaseContext context, TimeProvider time) : IScheduledJob
{
    // Every 5 minutes. Chosen because the reject rule is a 6-hour cutoff —
    // a coarser schedule (e.g. hourly) risks events sitting rejected/pending
    // up to an hour past the cutoff, which is sloppy for a same-day RSVP flow.
    // 5 minutes keeps both transitions close to real-time without hammering the DB.
    public static string? Schedule => "0 0/5 * ? * *";

    public static string Identity => nameof(EventStateJob);

    public async Task Execute(IJobExecutionContext job)
    {
        var now = time.GetUtcNow();
        var token = job.CancellationToken;

        var rejected = await RejectUnderThresholdPendingEventsAsync(now, token);
        if (rejected > 0)
            logger.LogInformation("Auto-rejected {count} under-threshold pending event(s).", rejected);

        var completed = await CompleteEndedEventsAsync(now, token);
        if (completed > 0)
            logger.LogInformation("Marked {count} event(s) as completed.", completed);
    }

    /// <summary>
    /// Auto-rejects Pending events starting within 6 hours whose attendee count
    /// hasn't reached their threshold. Events without a threshold are untouched
    /// since there's nothing to fail against.
    /// </summary>
    /// <param name="now"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task<int> RejectUnderThresholdPendingEventsAsync(DateTimeOffset now, CancellationToken token)
    {
        var cutoff = now.AddHours(6);

        return await context.Events
            .Where(e => e.State == EventState.Pending
                && e.Threshold != null
                && e.StartsAt <= cutoff
                && context.UserEvent.Count(ue => ue.EventId == e.Id) < e.Threshold)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.State, EventState.Rejected)
                .SetProperty(e => e.UpdatedAt, now), token);
    }

    /// <summary>
    /// Transitions Accepted events to Completed once their end time has passed.
    /// </summary>
    /// <param name="now"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task<int> CompleteEndedEventsAsync(DateTimeOffset now, CancellationToken token)
    {
        return await context.Events
            .Where(e => e.State == EventState.Accepted && e.EndsAt <= now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.State, EventState.Finished)
                .SetProperty(e => e.UpdatedAt, now), token);
    }
}