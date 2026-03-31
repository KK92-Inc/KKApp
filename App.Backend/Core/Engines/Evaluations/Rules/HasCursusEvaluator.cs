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

public sealed class HasCursusEvaluator(DatabaseContext db) : IRuleEvaluator<HasCursusRule>
{
    public async Task<Result> EvaluateAsync(HasCursusRule rule, Context ctx, CancellationToken ct)
    {
        var enrolled = await db.UserCursi
            .Include(uc => uc.Cursus)
            .AnyAsync(uc =>
                uc.UserId == ctx.User.Id &&
                uc.CursusId == rule.CursusId &&
                uc.State == EntityObjectState.Completed,
            ct);

        return enrolled
            ? Result.Success()
            : Result.Failure(rule.Description ?? $"Must be enrolled in cursus '{rule.CursusId}'.");
    }
}