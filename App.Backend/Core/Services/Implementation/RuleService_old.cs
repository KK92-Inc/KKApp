// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Rules;
using App.Backend.Core.Services.Interface;
using App.Backend.Database;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Rules;
using Microsoft.EntityFrameworkCore;

namespace App.Backend.Core.Services.Implementation;

/// <summary>
/// Service for evaluating eligibility rules.
/// </summary>
public class RuleService(DatabaseContext context) : IRuleService
{
    private readonly DatabaseContext _context = context;

    /// <inheritdoc />
    public async Task<RuleEngineResult> EvaluateAsync(
        IEnumerable<Rule> rules,
        RuleContext ctx,
        CancellationToken token = default)
    {
        var results = new List<RuleEngineResult>();

        foreach (var rule in rules)
        {
            var result = rule switch
            {
                MinReviewsCompletedRule r => await EvaluateMinReviewsCompletedAsync(r, ctx, token),
                MinProjectsCompletedRule r => await EvaluateMinProjectsCompletedAsync(r, ctx, token),
                MinDaysRegisteredRule r => EvaluateMinDaysRegistered(r, ctx),
                HasProjectRule r => await EvaluateHasCompletedProjectAsync(r, ctx, token),
                HasCursusRule r => await EvaluateEnrolledInCursusAsync(r, ctx, token),
                // HasCursusRoleRule r => await EvaluateHasCursusRoleAsync(r, ctx, token),
                SameTimezoneRule r => EvaluateSameTimezone(r, ctx),
                IsMemberRule r => await EvaluateIsTeamMemberAsync(r, ctx, token),
                AllOfRule r => await EvaluateAllOfAsync(r, ctx, token),
                AnyOfRule r => await EvaluateAnyOfAsync(r, ctx, token),
                NotRule r => await EvaluateNotAsync(r, ctx, token),
                _ => RuleEngineResult.Failure($"Unknown rule type: {rule.GetType().Name}")
            };
            results.Add(result);
        }

        return RuleEngineResult.Combine(results);
    }

    /// <inheritdoc />
    public async Task<RuleEngineResult> CanRequestReviewAsync(
        Rubric rubric,
        User reviewer,
        UserProject userProject,
        CancellationToken token = default)
    {
        var ctx = new RuleContext
        {
            User = reviewer,
            UserProject = userProject
        };

        return await EvaluateAsync(rubric.ReviewerEligibilityRules, ctx, token);
    }

    /// <inheritdoc />
    public async Task<RuleEngineResult> CanReviewAsync(
        Rubric rubric,
        UserProject userProject,
        User requestingUser,
        CancellationToken token = default)
    {
        var ctx = new RuleContext
        {
            User = requestingUser,
            UserProject = userProject
        };

        return await EvaluateAsync(rubric.RevieweeEligibilityRules, ctx, token);
    }

    /// <summary>
    /// Evaluates a single eligibility rule.
    /// </summary>
    private async Task<RuleEngineResult> EvaluateRuleAsync(
        Rule rule,
        RuleContext ctx,
        CancellationToken token)
    {
        return rule switch
        {
            MinReviewsCompletedRule r => await EvaluateMinReviewsCompletedAsync(r, ctx, token),
            MinProjectsCompletedRule r => await EvaluateMinProjectsCompletedAsync(r, ctx, token),
            MinDaysRegisteredRule r => EvaluateMinDaysRegistered(r, ctx),
            HasProjectRule r => await EvaluateHasCompletedProjectAsync(r, ctx, token),
            HasCursusRule r => await EvaluateEnrolledInCursusAsync(r, ctx, token),
            // HasCursusRoleRule r => await EvaluateHasCursusRoleAsync(r, ctx, token),
            SameTimezoneRule r => EvaluateSameTimezone(r, ctx),
            IsMemberRule r => await EvaluateIsTeamMemberAsync(r, ctx, token),
            AllOfRule r => await EvaluateAllOfAsync(r, ctx, token),
            AnyOfRule r => await EvaluateAnyOfAsync(r, ctx, token),
            NotRule r => await EvaluateNotAsync(r, ctx, token),
            _ => RuleEngineResult.Failure($"Unknown rule type: {rule.GetType().Name}")
        };
    }

    // ========================================================================
    // Count-based Rules
    // ========================================================================

    private async Task<RuleEngineResult> EvaluateMinReviewsCompletedAsync(
        MinReviewsCompletedRule rule,
        RuleContext ctx,
        CancellationToken token)
    {
        var completedReviews = await _context.Reviews
            .Where(r => r.ReviewerId == ctx.User.Id && r.State == ReviewState.Finished)
            .CountAsync(token);

        if (completedReviews >= rule.MinCount)
            return RuleEngineResult.Success();

        return RuleEngineResult.Failure(
            rule.Description ?? $"User must have completed at least {rule.MinCount} reviews (current: {completedReviews})");
    }

    private async Task<RuleEngineResult> EvaluateMinProjectsCompletedAsync(
        MinProjectsCompletedRule rule,
        RuleContext ctx,
        CancellationToken token)
    {
        var completedProjects = await _context.UserProjects
            .Where(up => up.Members.Any(m => m.UserId == ctx.User.Id) && up.State == EntityObjectState.Completed)
            .CountAsync(token);

        if (completedProjects >= rule.MinCount)
            return RuleEngineResult.Success();

        return RuleEngineResult.Failure(
            rule.Description ?? $"User must have completed at least {rule.MinCount} projects (current: {completedProjects})");
    }

    private RuleEngineResult EvaluateMinDaysRegistered(
        MinDaysRegisteredRule rule,
        RuleContext ctx)
    {
        var daysSinceRegistration = (ctx.Now - ctx.User.CreatedAt).Days;

        if (daysSinceRegistration >= rule.MinDays)
            return RuleEngineResult.Success();

        return RuleEngineResult.Failure(
            rule.Description ?? $"User must be registered for at least {rule.MinDays} days (current: {daysSinceRegistration})");
    }

    // ========================================================================
    // Project / Cursus-based Rules
    // ========================================================================

    private async Task<RuleEngineResult> EvaluateHasCompletedProjectAsync(
        HasProjectRule rule,
        RuleContext ctx,
        CancellationToken token)
    {
        var hasCompleted = await _context.UserProjects
            .Where(up => up.Members.Any(m => m.UserId == ctx.User.Id)
                         && up.State == EntityObjectState.Completed
                         && up.Project.Slug == rule.ProjectSlug)
            .AnyAsync(token);

        if (hasCompleted)
            return RuleEngineResult.Success();

        return RuleEngineResult.Failure(
            rule.Description ?? $"User must have completed project '{rule.ProjectSlug}'");
    }

    private async Task<RuleEngineResult> EvaluateEnrolledInCursusAsync(
        HasCursusRule rule,
        RuleContext ctx,
        CancellationToken token)
    {
        var isEnrolled = await _context.UserCursi
            .Where(uc => uc.UserId == ctx.User.Id
                         && uc.Cursus.Slug == rule.CursusSlug
                         && uc.State == EntityObjectState.Active)
            .AnyAsync(token);

        if (isEnrolled)
            return RuleEngineResult.Success();

        return RuleEngineResult.Failure(
            rule.Description ?? $"User must be enrolled in cursus '{rule.CursusSlug}'");
    }

    private async Task<RuleEngineResult> EvaluateHasCursusRoleAsync(
        HasCursusRoleRule rule,
        RuleContext ctx,
        CancellationToken token)
    {
        // Note: You'll need to add role checking logic based on your cursus role system
        // This is a placeholder implementation
        var hasRole = await _context.UserCursi
            .Where(uc => uc.UserId == ctx.User.Id && uc.Cursus.Slug == rule.CursusSlug)
            .AnyAsync(token);

        if (hasRole)
            return RuleEngineResult.Success();

        return RuleEngineResult.Failure(
            rule.Description ?? $"User must have role '{rule.Role}' in cursus '{rule.CursusSlug}'");
    }

    // ========================================================================
    // Contextual Rules
    // ========================================================================

    private RuleEngineResult EvaluateSameTimezone(SameTimezoneRule rule, RuleContext ctx)
    {
        // Note: This requires timezone info on users. Placeholder implementation.
        // You may need to add timezone fields to User entity.
        // For now, always passes - you can implement this based on your timezone strategy.
        return RuleEngineResult.Success();
    }

    private async Task<RuleEngineResult> EvaluateIsTeamMemberAsync(
        IsMemberRule rule,
        RuleContext ctx,
        CancellationToken token)
    {
        if (ctx.UserProject is null)
            return RuleEngineResult.Failure("No user project context provided for team membership check");

        var isMember = await _context.UserProjectMembers
            .Where(m => m.UserProjectId == ctx.UserProject.Id && m.UserId == ctx.User.Id)
            .AnyAsync(token);

        if (isMember)
            return RuleEngineResult.Success();

        return RuleEngineResult.Failure(
            rule.Description ?? "User must be a member of the project team");
    }

    // ========================================================================
    // Logical Composition Rules
    // ========================================================================

    private async Task<RuleEngineResult> EvaluateAllOfAsync(
        AllOfRule rule,
        RuleContext ctx,
        CancellationToken token)
    {
        var results = new List<RuleEngineResult>();

        foreach (var nestedRule in rule.Rules)
        {
            var result = await EvaluateRuleAsync(nestedRule, ctx, token);
            results.Add(result);
        }

        return RuleEngineResult.Combine(results);
    }

    private async Task<RuleEngineResult> EvaluateAnyOfAsync(
        AnyOfRule rule,
        RuleContext ctx,
        CancellationToken token)
    {
        var reasons = new List<string>();

        foreach (var nestedRule in rule.Rules)
        {
            var result = await EvaluateRuleAsync(nestedRule, ctx, token);
            if (result.IsEligible)
                return RuleEngineResult.Success();
            reasons.AddRange(result.Reasons);
        }

        return RuleEngineResult.Failure(
            rule.Description ?? $"At least one of the following conditions must be met: {string.Join("; ", reasons)}");
    }

    private async Task<RuleEngineResult> EvaluateNotAsync(
        NotRule rule,
        RuleContext ctx,
        CancellationToken token)
    {
        var result = await EvaluateRuleAsync(rule.Rule, ctx, token);

        if (!result.IsEligible)
            return RuleEngineResult.Success();

        return RuleEngineResult.Failure(
            rule.Description ?? "The following condition must NOT be met");
    }
}
