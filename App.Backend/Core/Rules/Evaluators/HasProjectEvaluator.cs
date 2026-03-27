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
/// Evaluates if a user has completed a specific project.
/// </summary>
public class HasProjectEvaluator(DatabaseContext db) : IRuleEvaluator<HasProjectRule>
{
    public async Task<RuleEngineResult> EvaluateAsync(HasProjectRule rule, RuleContext ctx, CancellationToken ct)
    {
        var hasCompleted = await db.UserProjects
            .Where(up => up.Members.Any(m => m.UserId == ctx.User.Id)
                && up.State == EntityObjectState.Completed
                && up.Project.Id == rule.ProjectId)
            .AnyAsync(ct);

        return hasCompleted
            ? RuleEngineResult.Success()
            : RuleEngineResult.Failure(rule.Description ?? $"Must have completed project {rule.ProjectId}");
    }
}
