// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================


using App.Backend.Core.Engines.Evaluations;
using App.Backend.Core.Engines.Evaluations.Enums;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Entities.Users;

namespace App.Backend.Core.Services.Implementation;

/// <summary>
/// Service for evaluating eligibility rules.
/// Delegates rule evaluation to the RuleDispatcher which routes to individual evaluators.
/// </summary>
public sealed class RuleService(RuleEngine engine) : IRuleService
{
    /// <inheritdoc />
    public async Task<Result> EvaluateAsync(IEnumerable<Rule> rules, Context ctx, CancellationToken ct = default)
    {
        return await engine.EvaluateAllAsync(rules, ctx, ct);
    }

    /// <inheritdoc />
    public async Task<Result> CanRequestReviewAsync(
        Rubric rubric, User reviewee, UserProject project, CancellationToken ct = default)
    {
        if (rubric.RevieweeRules.Count is 0)
            return Result.Success();

        return await engine.EvaluateAllAsync(rubric.RevieweeRules, new ()
        {
            User           = reviewee,
            Role           = Role.Reviewee,
            SubjectProject = project
        }, ct);
    }

    /// <inheritdoc />
    public async Task<Result> CanReviewAsync(
        Rubric rubric, User reviewer, UserProject subjectProject, CancellationToken ct = default)
    {
        if (rubric.ReviewerRules.Count is 0)
            return Result.Success();

        return await engine.EvaluateAllAsync(rubric.ReviewerRules, new ()
        {
            User           = reviewer,
            Role           = Role.Reviewer,
            SubjectProject = subjectProject
        }, ct);
    }
}
