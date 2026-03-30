// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Rules.Evaluations;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Core.Engines.Evaluations.Rules;

public sealed class MinReviewsCompletedEvaluator(DatabaseContext db) : IRuleEvaluator<MinReviewsCompletedRule>
{
    public async Task<Result> EvaluateAsync(MinReviewsCompletedRule rule, Context ctx, CancellationToken ct)
    {
        // TODO: Add column to see which ones are done
        var count = await db.Reviews.CountAsync(r => r.ReviewerId == ctx.User.Id && r.State == ReviewState.Finished, ct); // && r.CompletedAt != null, ct);

        return count >= rule.Count
            ? Result.Success()
            : Result.Failure(rule.Description
                ?? $"Must have completed at least {rule.Count} review(s) (you have {count}).");
    }
}