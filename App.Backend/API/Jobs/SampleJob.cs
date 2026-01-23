// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Quartz;
using App.Backend.API.Jobs.Interfaces;
using Wolverine;

// ============================================================================

namespace App.Backend.API.Jobs;

/// <summary>
/// A sample scheduled job that runs every minute.
/// </summary>
[DisallowConcurrentExecution]
public class SampleJob(ILogger<SampleJob> logger, IMessageBus bus) : IScheduledJob
{
    public static string? Schedule => "0 0/1 * ? * *";

    public static string Identity => nameof(SampleJob);

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Sample Job is running at {time}", DateTimeOffset.Now);
        await bus.PublishAsync(new UserRegistered(Guid.CreateVersion7()));
    }
}
