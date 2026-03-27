// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Rules;
using Microsoft.EntityFrameworkCore;

namespace App.Backend.Core.Rules.Evaluators;

/// <summary>
/// Evaluates if a user has completed at least a minimum number of reviews.
/// </summary>
public class MinReviewsCompletedEvaluator(DatabaseContext db) : IRuleEvaluator<MinReviewsCompletedRule>
{
    public async Task<RuleEngineResult> EvaluateAsync(MinReviewsCompletedRule rule, RuleContext ctx, CancellationToken ct)
    {
        var count = await db.Reviews
            .Where(r => r.ReviewerId == ctx.User.Id && r.State == ReviewState.Finished)
            .CountAsync(ct);

        return count >= rule.MinCount
            ? RuleEngineResult.Success()
            : RuleEngineResult.Failure(rule.Description ?? $"Must have completed at least {rule.MinCount} reviews (have {count})");
    }
}
