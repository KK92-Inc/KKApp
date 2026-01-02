// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Rules;
using App.Backend.Core.Services.Interface;
using App.Backend.Database;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Rules;
using Microsoft.EntityFrameworkCore;

namespace App.Backend.Core.Services.Implementation;

/// <summary>
/// Service for evaluating eligibility rules.
/// </summary>
public class RuleServiceN(DatabaseContext context) : IRuleService
{
    private readonly DatabaseContext _context = context;

    public async Task<RuleEngineResult> EvaluateAsync(IEnumerable<Rule> rules, RuleContext context, CancellationToken token = default)
    {
        var results = new List<RuleEngineResult>();
        await Parallel.ForEachAsync(rules, token, async (rule, ct) =>
        {
            results.Add(await EvaluateRuleAsync(rule, context, ct));
        });

        return RuleEngineResult.Combine(results);
    }

    public Task<RuleEngineResult> AbleToRequestReviewAsync(Rubric rubric, UserProject up, User user, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<RuleEngineResult> AbleToReviewAsync(Rubric rubric, User reviewer, UserProject up, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    #region Rule Evaluators

    private async Task<RuleEngineResult> EvaluateRuleAsync(Rule rule, RuleContext ctx, CancellationToken token)
    {
        return rule switch
        {
            MinReviewsCompletedRule r => await MinReviewsAsync(r, ctx, token),
            MinProjectsCompletedRule r => await MinProjectsAsync(r, ctx, token),
            MinDaysRegisteredRule r => MinDaysRegistered(r, ctx),
            HasProjectRule r => await HasCompletedProjectAsync(r, ctx, token),
            HasCursusRule r => await HasCursusAsync(r, ctx, token),
            SameTimezoneRule r => SameTimezone(r, ctx),
            IsMemberRule r => await IsMemberAsync(r, ctx, token),
            AllOfRule r => await AllOfAsync(r, ctx, token),
            AnyOfRule r => await AnyOfAsync(r, ctx, token),
            NotRule r => await NotAsync(r, ctx, token),
            _ => RuleEngineResult.Failure($"Unknown rule type: {rule.GetType().Name}")
        };
    }

    private async Task<RuleEngineResult> NotAsync(NotRule r, RuleContext context, CancellationToken ct)
    {
        var result = await EvaluateRuleAsync(r.Rule, context, ct);

        throw new NotImplementedException();
    }

    private async Task<RuleEngineResult> AnyOfAsync(AnyOfRule r, RuleContext context, CancellationToken ct)
    {
        var results = new List<RuleEngineResult>();

        foreach (var nestedRule in r.Rules)
            results.Add(await EvaluateRuleAsync(nestedRule, context, ct));
        return RuleEngineResult.Combine(results);
    }

    private async Task<RuleEngineResult> AllOfAsync(AllOfRule r, RuleContext context, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks if the user is a member of the specified project instance.
    /// </summary>
    /// <param name="r"></param>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<RuleEngineResult> IsMemberAsync(IsMemberRule r, RuleContext context, CancellationToken ct)
    {
        if (context.UserProject is null)
            return RuleEngineResult.Failure("User project not found");
        var isMember = await _context.UserProjectMembers
            .Where(m => m.UserId == context.User.Id && m.UserProjectId == context.UserProject.Id)
            .AnyAsync(ct);
        
        if (!isMember)
            return RuleEngineResult.Failure(r.Description ?? "User is not a member of the project instance");
        return RuleEngineResult.Success();
    }

    /// <summary>
    /// Checks if the user is in the same timezone as required.
    /// </summary>
    /// <param name="r"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private RuleEngineResult SameTimezone(SameTimezoneRule r, RuleContext context)
    {
        // Note: This requires timezone info on users. Placeholder implementation.
        return RuleEngineResult.Skip("Timezone information not available");
    }

    /// <summary>
    /// Checks if the user is enrolled in the specified cursus.
    /// </summary>
    /// <param name="r"></param>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<RuleEngineResult> HasCursusAsync(HasCursusRule r, RuleContext context, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks if the user has completed the specified project.
    /// </summary>
    /// <param name="r"></param>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    private async Task<RuleEngineResult> HasCompletedProjectAsync(HasProjectRule r, RuleContext context, CancellationToken ct)
    {
        var completed = await _context.UserProjects
            .Include(up => up.Project)
            .Include(up => up.Members)
            .Where(up => up.Members.Any(m => m.UserId == context.User.Id)
                && up.State == EntityObjectState.Completed
                && up.Project.Id == r.ProjectId)
            .AnyAsync(ct);

        if (!completed)
            return RuleEngineResult.Failure(r.Description ?? $"Must have completed project: {r.ProjectId}");
        return RuleEngineResult.Success();
    }

    private RuleEngineResult MinDaysRegistered(MinDaysRegisteredRule r, RuleContext context)
    {
        if ((context.Now - context.User.CreatedAt).Days >= r.MinDays)
            return RuleEngineResult.Success();
        return RuleEngineResult.Failure(r.Description ?? $"Expected: {r.MinDays}");
    }

    private async Task<RuleEngineResult> MinProjectsAsync(MinProjectsCompletedRule r, RuleContext context, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    private async Task<RuleEngineResult> MinReviewsAsync(MinReviewsCompletedRule r, RuleContext context, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    #endregion
}