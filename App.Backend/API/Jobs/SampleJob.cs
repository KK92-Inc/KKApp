using Quartz;

namespace App.Backend.API.Jobs;

[DisallowConcurrentExecution]
public class SampleJob(ILogger<SampleJob> logger) : IScheduledJob
{
    public static string? Schedule => "0 0/1 * ? * *";

    public static string Identity => nameof(SampleJob);

    public Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Sample Job is running at {time}", DateTimeOffset.Now);
        return Task.CompletedTask;
    }
}
