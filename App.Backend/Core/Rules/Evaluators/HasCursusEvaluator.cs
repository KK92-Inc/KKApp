// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Domain.Rules;
using Microsoft.EntityFrameworkCore;

namespace App.Backend.Core.Rules.Evaluators;

/// <summary>
/// Evaluates if a user is enrolled in a specific cursus.
/// </summary>
public class HasCursusEvaluator(DatabaseContext db) : IRuleEvaluator<HasCursusRule>
{
    public async Task<RuleEngineResult> EvaluateAsync(HasCursusRule rule, RuleContext ctx, CancellationToken ct)
    {
        var isEnrolled = await db.UserCursi
            .Where(uc => uc.UserId == ctx.User.Id && uc.Cursus.Slug == rule.CursusSlug)
            .AnyAsync(ct);

        return isEnrolled
            ? RuleEngineResult.Success()
            : RuleEngineResult.Failure(rule.Description ?? $"Must be enrolled in cursus '{rule.CursusSlug}'");
    }
}
