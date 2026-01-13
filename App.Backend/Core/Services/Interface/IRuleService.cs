// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Rules;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Rules;

namespace App.Backend.Core.Services.Interface;

/// <summary>
/// Service for evaluating eligibility rules.
/// </summary>
public interface IRuleService
{
    /// <summary>
    /// Evaluates a list of eligibility rules against a user context.
    /// All rules must pass for the user to be eligible.
    /// </summary>
    /// <param name="rules">The rules to evaluate.</param>
    /// <param name="context">The context containing user and project info.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The eligibility result.</returns>
    Task<RuleEngineResult> EvaluateAsync(
        IEnumerable<Rule> rules,
        RuleContext context,
        CancellationToken token = default
    );

    /// <summary>
    /// Determines whether a user is eligible to act as a reviewer for a rubric.
    /// </summary>
    /// <param name="rubric">The rubric with reviewer eligibility rules.</param>
    /// <param name="reviewer">The potential reviewer.</param>
    /// <param name="userProject">The user project to be reviewed.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The eligibility result.</returns>
    Task<RuleEngineResult> AbleToReviewAsync(
        Rubric rubric,
        User reviewer,
        UserProject userProject,
        CancellationToken token = default
    );

    /// <summary>
    /// Determines whether a user project (and its owner) are eligible to request a review using the rubric.
    /// </summary>
    /// <param name="rubric">The rubric with reviewee eligibility rules.</param>
    /// <param name="userProject">The user project requesting review.</param>
    /// <param name="user">The user making the request.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The eligibility result.</returns>
    Task<RuleEngineResult> AbleToRequestReviewAsync(
        Rubric rubric,
        User user,
        UserProject userProject,
        CancellationToken token = default
    );
}
