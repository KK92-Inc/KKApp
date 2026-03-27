// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Rules;

namespace App.Backend.Core.Rules.Evaluators;

/// <summary>
/// Evaluates a composite rule where ANY nested rule must pass (logical OR).
/// </summary>
public class AnyOfEvaluator(RuleDispatcher dispatcher) : IRuleEvaluator<AnyOfRule>
{
    public async Task<RuleEngineResult> EvaluateAsync(AnyOfRule rule, RuleContext ctx, CancellationToken ct)
    {
        var allReasons = new List<string>();

        foreach (var nestedRule in rule.Rules)
        {
            var result = await dispatcher.EvaluateAsync(nestedRule, ctx, ct);

            // Short-circuit on first success for AnyOf
            if (result.IsEligible)
                return RuleEngineResult.Success();

            allReasons.AddRange(result.Reasons);
        }

        // All rules failed
        return RuleEngineResult.Failure(rule.Description ?? $"None of the conditions were met: {string.Join("; ", allReasons)}");
    }
}
