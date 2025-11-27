// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Quartz;

// ============================================================================

namespace App.Backend.API.Jobs;

/// <summary>
/// Base shape for a Scheduled Job.
/// </summary>
public interface IScheduledJob : IJob
{
    /// <summary>
    /// A cron schedule to determine how often to trigger the job.
    ///
    /// If null, the job may only be triggered manually via the trigger.
    /// You may also use <see cref="CronScheduleBuilder"/> if it's easier.
    /// </summary>
    public static abstract string? Schedule { get; }

    /// <summary>
    /// The name / identity of the job.
    /// </summary>
    public static abstract string Identity { get; }
}

public static class JobService
{
    public static void Register<Job>(
        IServiceCollectionQuartzConfigurator quartz,
        ILogger<Job> logger
    ) where Job : IScheduledJob
    {
        try
        {
            JobKey jobKey = new(Job.Identity);
            quartz.AddJob<Job>(opts => opts.WithIdentity(jobKey));
            quartz.AddTrigger(opts =>
            {
                opts.ForJob(jobKey);
                opts.WithIdentity($"{Job.Identity}-trigger");
                if (Job.Schedule is not null)
                    opts.WithCronSchedule(Job.Schedule);
            });

        }
        catch (FormatException)
        {
            logger.LogError($"Failed to register job: {Job.Identity} due to badly formated cron: '{Job.Schedule}'");
            throw;
        }
    }
}
