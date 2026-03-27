// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Rules;

namespace App.Backend.Core.Rules.Evaluators;

/// <summary>
/// Evaluates a composite rule where ALL nested rules must pass (logical AND).
/// </summary>
public class AllOfEvaluator(RuleDispatcher dispatcher) : IRuleEvaluator<AllOfRule>
{
    public async Task<RuleEngineResult> EvaluateAsync(AllOfRule rule, RuleContext ctx, CancellationToken ct)
    {
        var results = new List<RuleEngineResult>();

        foreach (var nestedRule in rule.Rules)
        {
            var result = await dispatcher.EvaluateAsync(nestedRule, ctx, ct);
            results.Add(result);

            // Short-circuit on first failure for AllOf
            if (!result.IsEligible)
                break;
        }

        return RuleEngineResult.Combine(results);
    }
}
