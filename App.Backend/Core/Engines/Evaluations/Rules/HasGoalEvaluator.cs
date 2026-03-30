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

public sealed class HasGoalEvaluator(DatabaseContext db) : IRuleEvaluator<HasGoalRule>
{
    public async Task<Result> EvaluateAsync(HasGoalRule rule, Context ctx, CancellationToken ct)
    {
        var enrolled = await db.UserGoals
            .AnyAsync(ug => 
                ug.UserId == ctx.User.Id &&
                ug.GoalId == rule.GoalId &&
                ug.State == EntityObjectState.Completed, 
            ct);

        return enrolled
            ? Result.Success()
            : Result.Failure(rule.Description ?? $"Must have completed goal '{rule.GoalSlug}'.");
    }
}
