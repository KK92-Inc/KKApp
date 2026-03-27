// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Rules;

namespace App.Backend.Core.Rules;

/// <summary>
/// Dispatches rule evaluation to the appropriate evaluator based on rule type.
/// Acts as the central routing mechanism for the rule engine.
/// </summary>
public class RuleDispatcher
{
    private readonly Dictionary<Type, IRuleEvaluator> _evaluators;

    public RuleDispatcher(IEnumerable<IRuleEvaluator> evaluators)
    {
        _evaluators = evaluators.ToDictionary(e => e.RuleType, e => e);
    }

    /// <summary>
    /// Evaluates a single rule by dispatching to the appropriate evaluator.
    /// </summary>
    public Task<RuleEngineResult> EvaluateAsync(Rule rule, RuleContext ctx, CancellationToken ct)
    {
        var ruleType = rule.GetType();

        if (!_evaluators.TryGetValue(ruleType, out var evaluator))
            return Task.FromResult(RuleEngineResult.Failure($"No evaluator registered for rule type: {ruleType.Name}"));

        return evaluator.EvaluateAsync(rule, ctx, ct);
    }

    /// <summary>
    /// Evaluates multiple rules. All rules must pass (logical AND).
    /// </summary>
    public async Task<RuleEngineResult> EvaluateAllAsync(IEnumerable<Rule> rules, RuleContext ctx, CancellationToken ct)
    {
        var results = new List<RuleEngineResult>();

        foreach (var rule in rules)
        {
            var result = await EvaluateAsync(rule, ctx, ct);
            results.Add(result);

            // Short-circuit on failure
            if (!result.IsEligible)
                break;
        }

        return RuleEngineResult.Combine(results);
    }
}
