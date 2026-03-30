// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain;
using App.Backend.Domain.Rules.Evaluations.Composites;

// ============================================================================

namespace App.Backend.Core.Engines.Evaluations;

/// <summary>
/// Dispatches rules to their evaluators.
/// Inject this anywhere you need ad-hoc evaluation outside of RuleService.
/// </summary>
public sealed class RuleEngine(IEnumerable<IRuleEvaluator> evaluators)
{
    private readonly IReadOnlyDictionary<Type, IRuleEvaluator> _evaluators = evaluators.ToDictionary(e => e.RuleType);

    /// <summary>
    /// Evaluates a rule against the provided context.
    /// Handles composite rules (AllOf, AnyOf, Not) structurally; dispatches leaf rules to their evaluators.
    /// </summary> <param name="rule">The rule to evaluate.</param>
    /// <param name="ctx">The context for evaluation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result of the evaluation.</returns>
    public async Task<Result> EvaluateAsync(Rule rule, Context ctx, CancellationToken ct = default)
    {
        // Composites are structural — handle them here, not via evaluators
        return rule switch
        {
            AllOfRule allOf   => await EvaluateAllAsync(allOf.Rules, ctx, ct),
            AnyOfRule anyOf   => await EvaluateAnyAsync(anyOf.Rules, ctx, ct),
            NotRule not       => Invert(await EvaluateAsync(not.Rule, ctx, ct)),
            BranchRule branch => await EvaluateBranchAsync(branch, ctx, ct),
            _ => await DispatchAsync(rule, ctx, ct),
        };
    }

    /// <summary>
    /// Evaluates all rules and combines their results. Fails if any rule fails.
    /// </summary>
    /// <param name="rules">Rules to evaluate.</param>
    /// <param name="ctx">Evaluation context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success if all rules pass; otherwise, a combined failure result.</returns
    public async Task<Result> EvaluateAllAsync(IEnumerable<Rule> rules, Context ctx, CancellationToken ct = default)
    {
        var failures = new List<Result>();
        foreach (var rule in rules)
        {
            var result = await EvaluateAsync(rule, ctx, ct);
            if (!result.IsSuccess) failures.Add(result);
        }
        return failures.Count == 0 ? Result.Success() : Result.Combine(failures);
    }

    /// <summary>
    /// Evaluates rules and returns success if any rule passes. Fails if all rules fail
    /// and combines failure messages.
    /// </summary>
    /// <param name="rules">Rules to evaluate.</param>
    /// <param name="ctx">Evaluation context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success if any rule passes; otherwise, a combined failure result.</returns
    private async Task<Result> EvaluateAnyAsync(IEnumerable<Rule> rules, Context ctx, CancellationToken ct)
    {
        var failures = new List<Result>();
        foreach (var rule in rules)
        {
            var result = await EvaluateAsync(rule, ctx, ct);
            if (result.IsSuccess) return Result.Success();
            failures.Add(result);
        }
        return Result.Combine(failures);
    }

    private async Task<Result> EvaluateBranchAsync(BranchRule branch, Context ctx, CancellationToken ct)
    {
        var condition = await EvaluateAsync(branch.Condition, ctx, ct);
        return condition.IsSuccess
            ? await EvaluateAsync(branch.Then, ctx, ct)
            : await EvaluateAsync(branch.Else, ctx, ct);
    }

    private async Task<Result> DispatchAsync(Rule rule, Context ctx, CancellationToken ct)
    {
        if (!_evaluators.TryGetValue(rule.GetType(), out var evaluator))
            return Result.Failure($"No evaluator registered for '{rule.GetType().Name}'.");

        return await evaluator.EvaluateAsync(rule, ctx, ct);
    }

    private static Result Invert(Result result)
    {
        if (result.IsSkipped) return result; // Don't invert skipped rules
        return result.IsSuccess ? Result.Failure("Rule must not pass.") : Result.Success();
    }
}