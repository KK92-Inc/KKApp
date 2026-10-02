// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Quartz;
using App.Backend.API.Jobs.Interfaces;
using App.Backend.Core;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using App.Backend.Database;

// ============================================================================

namespace App.Backend.API.Jobs;

/// <summary>
/// Releases reviews that someone committed to but never carried out.
///
/// Nothing is stored for this, staleness is derived from the review's state and timestamps:
/// <list type="bullet">
/// <item>In progress for more than <see cref="FinishWindow"/> since it was started: stale. Any kind.</item>
/// <item>Claimed but still pending <see cref="StartWindow"/> after the claim: stale. Unclaimed slots never
/// go stale, the round simply waits for a reviewer.</item>
/// </list>
/// Self reviews never go stale while pending. Auto reviews are driven by the
/// system rather than a person, so they are skipped here.
///
/// Stale reviews go through <see cref="IReviewService.StopReviewAsync"/>: slots of an open round are released
/// for somebody else, advisory reviews are cancelled.
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

    // TODO: Make configurable via system row.
    // A rolling 2 days from the claim, i.e. "today and tomorrow", without caring about timezones.
    private static readonly TimeSpan StartWindow = TimeSpan.FromDays(2);
    private static readonly TimeSpan FinishWindow = TimeSpan.FromHours(24);

    public async Task Execute(IJobExecutionContext context)
    {
        var now = time.GetUtcNow();
        var notStartedBefore = now - StartWindow;
        var notFinishedBefore = now - FinishWindow;

        // A review is InProgress only via StartReviewAsync, which sets StartedAt,
        // and a non-Self review has a reviewer only via a claim, which sets ClaimedAt (releasing clears both).
        // Reviews of a closed round are left alone, StopReviewAsync refuses those (and nothing
        // is left in progress there, closing a round cancels its unfinished reviews).
        var stale = await ctx.Reviews
            .AsNoTracking()
            .Where(r => r.Kind != ReviewKinds.Auto)
            .Where(r => r.RoundId == null || r.Round!.State == ReviewRoundState.Open)
            .Where(r =>
                (r.State == ReviewState.InProgress && r.StartedAt <= notFinishedBefore) ||
                (r.State == ReviewState.Pending
                    && r.Kind != ReviewKinds.Self
                    && r.ReviewerId != null
                    && r.ClaimedAt <= notStartedBefore))
            .Select(r => new { r.Id, r.State })
            .ToListAsync(context.CancellationToken);

        if (stale.Count == 0)
            return;

        logger.LogInformation("Found {Count} stale review(s). Cleaning up...", stale.Count);

        var released = 0;
        foreach (var review in stale)
        {
            try
            {
                await reviewService.StopReviewAsync(review.Id, context.CancellationToken);
                released++;
            }
            catch (ServiceException ex)
            {
                // Most likely finished or cancelled by a person between our query and now.
                logger.LogInformation("Skipped stale review {ReviewId} ({State}): {Reason}", review.Id, review.State, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to clean up stale review {ReviewId}: {Reason}", review.Id, ex.Message);
            }
        }

        logger.LogInformation("Released {Count}/{Total} stale review(s)", released, stale.Count);
    }
}
