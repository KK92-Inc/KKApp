// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Rules;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Rules;

namespace App.Backend.Core.Services.Implementation;

/// <summary>
/// Service for evaluating eligibility rules.
/// Delegates rule evaluation to the RuleDispatcher which routes to individual evaluators.
/// </summary>
public class RuleService(RuleDispatcher dispatcher) : IRuleService
{
    /// <inheritdoc />
    public Task<RuleEngineResult> EvaluateAsync(IEnumerable<Rule> rules, RuleContext context, CancellationToken token = default)
    {
        return dispatcher.EvaluateAllAsync(rules, context, token);
    }

    /// <inheritdoc />
    public Task<RuleEngineResult> AbleToRequestReviewAsync(Rubric rubric, User user, UserProject userProject, CancellationToken token = default)
    {
        // If no reviewee rules are configured, allow by default
        if (rubric.RevieweeRules.Count == 0)
            return Task.FromResult(RuleEngineResult.Success());

        var context = new RuleContext
        {
            User = user,
            UserProject = userProject
        };

        return dispatcher.EvaluateAllAsync(rubric.RevieweeRules, context, token);
    }

    /// <inheritdoc />
    public Task<RuleEngineResult> AbleToReviewAsync(Rubric rubric, User reviewer, UserProject userProject, CancellationToken token = default)
    {
        // If no reviewer rules are configured, allow by default
        if (rubric.ReviewerRules.Count == 0)
            return Task.FromResult(RuleEngineResult.Success());

        var context = new RuleContext
        {
            User = reviewer,
            UserProject = userProject
        };

        return dispatcher.EvaluateAllAsync(rubric.ReviewerRules, context, token);
    }
}
