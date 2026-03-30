// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain;

namespace App.Backend.Core.Engines.Evaluations;

/// <summary>Marker interface used for DI scanning.</summary>
public interface IRuleEvaluator
{
    Type RuleType { get; }

    Task<Result> EvaluateAsync(Rule rule, Context ctx, CancellationToken ct);
}

/// <summary>
/// Implement this to add a new rule. Register it nowhere — the engine finds it automatically.
/// </summary>
public interface IRuleEvaluator<TRule> : IRuleEvaluator where TRule : Rule
{
    Task<Result> EvaluateAsync(TRule rule, Context ctx, CancellationToken ct);
    Type IRuleEvaluator.RuleType => typeof(TRule);
    
    Task<Result> IRuleEvaluator.EvaluateAsync(Rule rule, Context ctx, CancellationToken ct) => EvaluateAsync((TRule)rule, ctx, ct);
}