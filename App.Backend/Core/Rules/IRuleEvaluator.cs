// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Rules;

namespace App.Backend.Core.Rules;

/// <summary>
/// Base interface for rule evaluators. Used for DI registration scanning.
/// </summary>
public interface IRuleEvaluator
{
    /// <summary>
    /// The type of rule this evaluator handles.
    /// </summary>
    Type RuleType { get; }

    /// <summary>
    /// Evaluates the rule against the given context.
    /// </summary>
    Task<RuleEngineResult> EvaluateAsync(Rule rule, RuleContext ctx, CancellationToken ct);
}

/// <summary>
/// Strongly-typed rule evaluator for a specific rule type.
/// Implement this interface to create a new rule evaluator.
/// </summary>
/// <typeparam name="TRule">The rule type this evaluator handles.</typeparam>
public interface IRuleEvaluator<TRule> : IRuleEvaluator where TRule : Rule
{
    /// <summary>
    /// Evaluates the typed rule against the given context.
    /// </summary>
    Task<RuleEngineResult> EvaluateAsync(TRule rule, RuleContext ctx, CancellationToken ct);

    // Default implementations for base interface
    Type IRuleEvaluator.RuleType => typeof(TRule);

    Task<RuleEngineResult> IRuleEvaluator.EvaluateAsync(Rule rule, RuleContext ctx, CancellationToken ct)
        => EvaluateAsync((TRule)rule, ctx, ct);
}
