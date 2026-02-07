// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Quartz;
using App.Backend.API.Jobs.Interfaces;
using Wolverine;
using App.Backend.Core.Services.Interface;
using App.Backend.API.Notifications.Variants;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.API.Jobs;

/// <summary>
/// A sample scheduled job that runs every minute.
/// </summary>
[DisallowConcurrentExecution]
public class SampleJob(ILogger<SampleJob> logger, IMessageBus bus, IUserService user) : IScheduledJob
{
    public static string? Schedule => "0 0/1 * ? * *";

    public static string Identity => nameof(SampleJob);

    public async Task Execute(IJobExecutionContext context)
    {
        var usr = await user
            .Query(false)
            .Include(u => u.Details)
            .FirstOrDefaultAsync(u => u.Login == "demo");

        if (usr is null) return;

        await bus.PublishAsync(new WelcomeUserNotification(usr));
        logger.LogInformation("Sample Job is running at {time}", DateTimeOffset.Now);
    }
}
