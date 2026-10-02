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
        // NOTE: Only reviews that were part of an evaluation round count, advisory feedback
        // is excluded so it can't be farmed to become eligible.
        var count = await db.Reviews.CountAsync(r =>
            r.ReviewerId == ctx.User.Id &&
            r.State == ReviewState.Finished &&
            r.RoundId != null, ct);

        return count >= rule.Count
            ? Result.Success()
            : Result.Failure(rule.Description
                ?? $"Must have completed at least {rule.Count} review(s) (you have {count}).");
    }
}