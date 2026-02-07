// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Quartz;
using App.Backend.API.Jobs.Interfaces;

// ============================================================================

namespace App.Backend.API.Jobs.Extensions;

public static class JobExtensions
{
    extension(IServiceCollectionQuartzConfigurator quartz)
    {
        public void Register<TJob>() where TJob : IScheduledJob
        {
            JobKey jobKey = new(TJob.Identity);
            quartz.AddJob<TJob>(o => o.WithIdentity(jobKey));
            quartz.AddTrigger(o =>
            {
                o.ForJob(jobKey).WithIdentity($"{TJob.Identity}-trigger");
                if (TJob.Schedule is not null)
                    o.WithCronSchedule(TJob.Schedule);
            });
        }
    }
}
