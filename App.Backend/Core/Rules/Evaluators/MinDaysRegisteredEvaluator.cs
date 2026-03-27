// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Rules;

namespace App.Backend.Core.Rules.Evaluators;

/// <summary>
/// Evaluates if a user has been registered for at least a minimum number of days.
/// </summary>
public class MinDaysRegisteredEvaluator : IRuleEvaluator<MinDaysRegisteredRule>
{
    public Task<RuleEngineResult> EvaluateAsync(MinDaysRegisteredRule rule, RuleContext ctx, CancellationToken ct)
    {
        var daysSinceRegistration = (ctx.Now - ctx.User.CreatedAt).Days;

        var result = daysSinceRegistration >= rule.MinDays
            ? RuleEngineResult.Success()
            : RuleEngineResult.Failure(rule.Description ?? $"Must be registered for at least {rule.MinDays} days (registered {daysSinceRegistration} days ago)");

        return Task.FromResult(result);
    }
}
