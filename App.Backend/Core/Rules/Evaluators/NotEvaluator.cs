// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Rules;

namespace App.Backend.Core.Rules.Evaluators;

/// <summary>
/// Evaluates a composite rule that negates the nested rule (logical NOT).
/// </summary>
public class NotEvaluator(RuleDispatcher dispatcher) : IRuleEvaluator<NotRule>
{
    public async Task<RuleEngineResult> EvaluateAsync(NotRule rule, RuleContext ctx, CancellationToken ct)
    {
        var result = await dispatcher.EvaluateAsync(rule.Rule, ctx, ct);

        // Invert the result
        return result.IsEligible
            ? RuleEngineResult.Failure(rule.Description ?? "Condition must NOT be met")
            : RuleEngineResult.Success();
    }
}
