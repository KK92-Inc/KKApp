// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Quartz;
using App.Backend.API.Jobs.Interfaces;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using App.Backend.Database;

// ============================================================================

namespace App.Backend.API.Jobs;

/// <summary>
/// Cleans up abandoned reviews: reviews that are marked as in progress and not completed
/// within 24 hours will automatically be stopped/released or cancelled.
/// </summary>
[DisallowConcurrentExecution]
public class CleanupReviews(
    ILogger<CleanupReviews> logger,
    IReviewService reviewService,
    DatabaseContext ctx,
    TimeProvider time
) : IScheduledJob
{
    public static string? Schedule => "0 */10 * ? * *"; // Every 10 minutes

    public static string Identity => nameof(CleanupReviews);

    public async Task Execute(IJobExecutionContext context)
    {
        var cutoff = time.GetUtcNow().AddHours(-24);

        // Fetch reviews stuck in InProgress for more than 24 hours
        var stale = await ctx.Reviews
            .Where(r => r.State == ReviewState.InProgress && r.UpdatedAt <= cutoff)
            .Select(r => new { r.Id })
            .ToListAsync(context.CancellationToken);

        if (stale.Count == 0)
            return;

        logger.LogInformation("Found {Count} stale in-progress review(s) older than 24 hours. Cleaning up...", stale.Count);

        var removed = 0;
        foreach (var review in stale)
        {
            try
            {
                await reviewService.StopReviewAsync(review.Id, context.CancellationToken);
                removed++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to clean up stale review {ReviewId}: {reason}", review.Id, ex.Message);
            }
        }

        logger.LogInformation("Successfully cleaned up {Count}/{Total} stale review(s)", removed, stale.Count);
    }
}