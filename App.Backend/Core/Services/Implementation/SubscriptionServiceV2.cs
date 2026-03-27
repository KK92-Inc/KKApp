// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Core.Services.Options;
using App.Backend.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class SubscriptionServiceV2(DatabaseContext ctx, IOptions<SubscriptionOptions> options) : ISubscriptionService
{
    private readonly SubscriptionOptions config = options.Value;

    public async Task<bool> CanSubscribeToCursusAsync(Guid userId, Guid cursusId, CancellationToken token)
    {
        if (config.Mode is ProgressionMode.Free)
            return true;

        return true;
    }

    public async Task<bool> CanSubscribeToGoalAsync(Guid userId, Guid goalId, CancellationToken token)
    {
        // In Free mode, anyone can subscribe to any goal regardless of its relationships.
        if (config.Mode is ProgressionMode.Free)
            return true;

        // Check if already subscribed (applies regardless of cursus relationship)
        var userGoal = await ctx.UserGoals.FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GoalId == goalId, token);
        if (userGoal?.State is EntityObjectState.Awaiting or EntityObjectState.Active or EntityObjectState.Completed)
            return false; // Already subscribed or awaiting approval

        // Orphan goals (not linked to any cursus) can be subscribed to freely.
        var cursusGoals = await ctx.CursusGoal
            .Where(cg => cg.GoalId == goalId)
            .ToListAsync(token);

        if (cursusGoals.Count == 0)
            return true;

        // Get cursi that the user is enrolled in (Active or Awaiting)
        var enrolledCursusIds = await ctx.UserCursi
            .Where(uc => uc.UserId == userId && (uc.State == EntityObjectState.Active || uc.State == EntityObjectState.Awaiting))
            .Select(uc => uc.CursusId)
            .ToListAsync(token);

        // Filter to cursi that both contain this goal AND the user is enrolled in
        var relevantCursusGoals = cursusGoals
            .Where(cg => enrolledCursusIds.Contains(cg.CursusId))
            .ToList();

        // User must be enrolled in at least one cursus that uses this goal
        if (relevantCursusGoals.Count == 0)
            return false;

        // Check hierarchical and choice group rules for each relevant cursus
        // User can subscribe if they meet criteria in ANY of their enrolled cursi
        foreach (var cursusGoal in relevantCursusGoals)
        {
            if (await CanAdvanceToGoalInCursusAsync(userId, cursusGoal, token))
                return true;
        }

        return false;
    }

    private async Task<bool> CanAdvanceToGoalInCursusAsync(Guid userId, Domain.Relations.CursusGoal cursusGoal, CancellationToken token)
    {
        // Check 1: If there's a parent goal, it must be completed
        if (cursusGoal.ParentGoalId.HasValue)
        {
            var parentUserGoal = await ctx.UserGoals
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GoalId == cursusGoal.ParentGoalId.Value, token);

            if (parentUserGoal?.State is not EntityObjectState.Completed)
                return false; // Parent not completed
        }

        // Check 2: If this goal is part of a choice group, no sibling in the group should be chosen
        if (cursusGoal.ChoiceGroup.HasValue)
        {
            // Get all sibling goals in the same choice group (same cursus, same choice_group, but different goal)
            var siblingGoalIds = await ctx.CursusGoal
                .Where(cg =>
                    cg.CursusId == cursusGoal.CursusId &&
                    cg.ChoiceGroup == cursusGoal.ChoiceGroup &&
                    cg.GoalId != cursusGoal.GoalId)
                .Select(cg => cg.GoalId)
                .ToListAsync(token);

            // Check if user has already chosen any sibling goal
            var hasChosenSibling = await ctx.UserGoals
                .AnyAsync(ug =>
                    ug.UserId == userId &&
                    siblingGoalIds.Contains(ug.GoalId) &&
                    (ug.State == EntityObjectState.Active ||
                     ug.State == EntityObjectState.Awaiting ||
                     ug.State == EntityObjectState.Completed),
                    token);

            if (hasChosenSibling)
                return false; // Already chose a different goal in this choice group
        }

        return true;
    }

    public async Task<bool> CanSubscribeToProjectAsync(Guid userId, Guid projectId, CancellationToken token)
    {
        if (config.Mode is ProgressionMode.Free)
            return true;

        return true;
    }

    public async Task<UserCursus> SubscribeToCursusAsync(Guid userId, Guid cursusId, CancellationToken token)
    {
        var userCursus = await ctx.UserCursi.FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CursusId == cursusId, token);
        if (userCursus is not null)
        {
            if (userCursus.State is EntityObjectState.Active)
                throw new ServiceException("Already subscribed to this cursus.");
            if (userCursus.State is EntityObjectState.Completed)
                throw new ServiceException("Cannot resubscribe to a completed cursus.");
            if (userCursus.State is EntityObjectState.Awaiting)
                throw new ServiceException("Subscription is awaiting approval.");

            // Reactivate an unsubscribed cursus
            userCursus.State = EntityObjectState.Active;
            ctx.UserCursi.Update(userCursus);
            await ctx.SaveChangesAsync(token);
            return userCursus;
        }

        var result = await ctx.UserCursi.AddAsync(new()
        {
            CursusId = cursusId,
            UserId = userId,
        }, token);

        await ctx.SaveChangesAsync(token);
        return result.Entity;
    }

    public Task<UserGoal> SubscribeToGoalAsync(Guid userId, Guid goalId, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<UserProject> SubscribeToProjectAsync(Guid userId, Guid projectId, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task UnsubscribeFromCursusAsync(Guid userId, Guid cursusId, CancellationToken token)
    {
        var userCursus = ctx.UserCursi.FirstOrDefault(uc => uc.UserId == userId && uc.CursusId == cursusId);
        if (userCursus is null || userCursus.State is EntityObjectState.Inactive)
            throw new ServiceException(409, "Not subscribed to this cursus.");

        userCursus.State = EntityObjectState.Inactive;
        ctx.UserCursi.Update(userCursus);
        await ctx.SaveChangesAsync(token);
    }

    public Task UnsubscribeFromGoalAsync(Guid userId, Guid goalId, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task UnsubscribeFromProjectAsync(Guid userId, Guid projectId, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}