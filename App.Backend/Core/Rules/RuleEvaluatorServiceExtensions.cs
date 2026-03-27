// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Rules.Evaluators;
using Microsoft.Extensions.DependencyInjection;

namespace App.Backend.Core.Rules;

/// <summary>
/// Extension methods for registering rule evaluators in DI.
/// </summary>
public static class RuleEvaluatorServiceExtensions
{
    /// <summary>
    /// Registers all rule evaluators and the rule dispatcher.
    /// Call this in your service registration.
    /// </summary>
    public static IServiceCollection AddRuleEvaluators(this IServiceCollection services)
    {
        // Register the dispatcher
        services.AddScoped<RuleDispatcher>();

        // Register all evaluators - they implement IRuleEvaluator
        // Simple evaluators (no dependencies or only DbContext)
        services.AddScoped<IRuleEvaluator, MinReviewsCompletedEvaluator>();
        services.AddScoped<IRuleEvaluator, MinProjectsCompletedEvaluator>();
        services.AddScoped<IRuleEvaluator, MinDaysRegisteredEvaluator>();
        services.AddScoped<IRuleEvaluator, HasProjectEvaluator>();
        services.AddScoped<IRuleEvaluator, HasCursusEvaluator>();
        services.AddScoped<IRuleEvaluator, IsMemberEvaluator>();
        services.AddScoped<IRuleEvaluator, SameTimezoneEvaluator>();

        // Composite evaluators (depend on RuleDispatcher for nested rules)
        services.AddScoped<IRuleEvaluator, AllOfEvaluator>();
        services.AddScoped<IRuleEvaluator, AnyOfEvaluator>();
        services.AddScoped<IRuleEvaluator, NotEvaluator>();

        return services;
    }
}
