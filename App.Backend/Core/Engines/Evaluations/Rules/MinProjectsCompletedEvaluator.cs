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

public sealed class MinProjectsCompletedEvaluator(DatabaseContext db) : IRuleEvaluator<MinProjectsCompletedRule>
{
    public async Task<Result> EvaluateAsync(MinProjectsCompletedRule rule, Context ctx, CancellationToken ct)
    {
        var count = await db.UserProjectMembers
            .CountAsync(m =>
                m.UserId == ctx.User.Id &&
                m.LeftAt == null &&
                m.UserProject.State == EntityObjectState.Completed, ct);

        return count >= rule.Count
            ? Result.Success()
            : Result.Failure(rule.Description
                ?? $"Must have completed at least {rule.Count} project(s) (you have {count}).");
    }
}
