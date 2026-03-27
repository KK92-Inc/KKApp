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
/// Evaluates if a user has completed at least a minimum number of projects.
/// </summary>
public class MinProjectsCompletedEvaluator(DatabaseContext db) : IRuleEvaluator<MinProjectsCompletedRule>
{
    public async Task<RuleEngineResult> EvaluateAsync(MinProjectsCompletedRule rule, RuleContext ctx, CancellationToken ct)
    {
        var count = await db.UserProjects
            .Where(up => up.Members.Any(m => m.UserId == ctx.User.Id) && up.State == EntityObjectState.Completed)
            .CountAsync(ct);

        return count >= rule.MinCount
            ? RuleEngineResult.Success()
            : RuleEngineResult.Failure(rule.Description ?? $"Must have completed at least {rule.MinCount} projects (have {count})");
    }
}
