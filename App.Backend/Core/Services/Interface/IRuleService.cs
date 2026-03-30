// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Entities.Users;
using App.Backend.Core.Engines.Evaluations;

namespace App.Backend.Core.Services.Interface;

/// <summary>
/// Service for evaluating eligibility rules.
/// </summary>
public interface IRuleService
{
    /// <summary>
    /// Ad-Hoc evaluation of a list of rules in a given context.
    /// </summary>
    /// <param name="rules"></param>
    /// <param name="ctx"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<Result> EvaluateAsync(IEnumerable<Rule> rules, Context ctx, CancellationToken ct = default);

    /// <summary>
    /// Can this user request a review for the given project?
    /// </summary>
    /// <param name="rubric"></param>
    /// <param name="reviewee"></param>
    /// <param name="project"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<Result> CanRequestReviewAsync(Rubric rubric, User reviewee, UserProject project, CancellationToken ct = default);

    /// <summary>
    /// Can this user review the given project?
    /// </summary>
    /// <param name="rubric"></param>
    /// <param name="reviewer"></param>
    /// <param name="subjectProject"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<Result> CanReviewAsync(Rubric rubric, User reviewer, UserProject subjectProject, CancellationToken ct = default);
}
