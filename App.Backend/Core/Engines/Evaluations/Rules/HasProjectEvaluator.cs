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

public sealed class HasProjectEvaluator(DatabaseContext db) : IRuleEvaluator<HasProjectRule>
{
    public async Task<Result> EvaluateAsync(HasProjectRule rule, Context ctx, CancellationToken ct)
    {
        var hasProject = await db.UserProjectMembers
            .Include(m => m.UserProject)
            .AnyAsync(m =>
                m.UserId == ctx.User.Id &&
                m.LeftAt == null &&
                m.UserProject.ProjectId == rule.ProjectId &&
                m.UserProject.State == EntityObjectState.Completed, ct);

        return hasProject
            ? Result.Success()
            : Result.Failure(rule.Description ?? $"Must have completed project '{rule.ProjectId}'.");
    }
}
