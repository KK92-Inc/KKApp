// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Domain.Rules;
using Microsoft.EntityFrameworkCore;

namespace App.Backend.Core.Rules.Evaluators;

/// <summary>
/// Evaluates if a user is a member of the target project instance.
/// </summary>
public class IsMemberEvaluator(DatabaseContext db) : IRuleEvaluator<IsMemberRule>
{
    public async Task<RuleEngineResult> EvaluateAsync(IsMemberRule rule, RuleContext ctx, CancellationToken ct)
    {
        if (ctx.UserProject is null)
            return RuleEngineResult.Failure("No project context available");

        var isMember = await db.UserProjectMembers
            .Where(m => m.UserId == ctx.User.Id && m.UserProjectId == ctx.UserProject.Id && m.LeftAt == null)
            .AnyAsync(ct);

        return isMember
            ? RuleEngineResult.Success()
            : RuleEngineResult.Failure(rule.Description ?? "Must be a member of this project");
    }
}
