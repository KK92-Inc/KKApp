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

/// <summary>
/// Checks if the user is a member of the subject project being reviewed.
/// </summary>
public sealed class IsMemberEvaluator(DatabaseContext db) : IRuleEvaluator<IsMemberRule>
{
    public async Task<Result> EvaluateAsync(IsMemberRule rule, Context ctx, CancellationToken ct)
    {
        if (ctx.SubjectProject is null)
            return Result.Skip("No subject project in context; IsMember rule is not applicable.");

        var isMember = await db.Members
            .AnyAsync(m =>
                m.EntityId == ctx.SubjectProject.Id &&
                m.EntityType == MemberEntityType.UserProject &&
                m.UserId == ctx.User.Id &&
                m.LeftAt == null, ct);

        return isMember
            ? Result.Success()
            : Result.Failure(rule.Description ?? "Must be a member of the project.");
    }
}
