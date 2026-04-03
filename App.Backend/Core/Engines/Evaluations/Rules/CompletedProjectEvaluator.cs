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

public sealed class CompletedProjectEvaluator(DatabaseContext db) : IRuleEvaluator<HasProjectRule>
{
    public async Task<Result> EvaluateAsync(HasProjectRule rule, Context ctx, CancellationToken ct)
    {
        var completed = await db.Members
            .Join(db.UserProjects,
                member => member.EntityId,
                userProject => userProject.Id,
                (member, userProject) => new { member, userProject })
            .AnyAsync(joined =>
                joined.member.UserId == ctx.User.Id &&
                joined.member.LeftAt == null &&
                joined.member.EntityType == MemberEntityType.UserProject &&
                joined.userProject.ProjectId == rule.ProjectId &&
                joined.userProject.State == EntityObjectState.Completed,
            ct);
        
        if (completed)
            return Result.Success();
        return Result.Failure(rule.Description ?? $"Must have completed project '{rule.ProjectId}'.");
    }
}
