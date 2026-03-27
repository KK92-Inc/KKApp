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
using App.Backend.Domain.Relations;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class SubscriptionService(DatabaseContext ctx, IOptions<SubscriptionOptions> options) : ISubscriptionService
{
    private readonly SubscriptionOptions config = options.Value;

    public async Task<bool> CanSubscribeToCursusAsync(Guid userId, Guid cursusId, CancellationToken token = default)
    {
        if (config.Mode is ProgressionMode.Free)
            return true;

        return true;
    }

    public async Task<bool> CanSubscribeToGoalAsync(Guid userId, Guid goalId, CancellationToken token = default)
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

    private async Task<bool> CanAdvanceToGoalInCursusAsync(Guid userId, CursusGoal cursusGoal, CancellationToken token = default)
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

    public async Task<bool> CanSubscribeToProjectAsync(Guid userId, Guid projectId, CancellationToken token = default)
    {
        if (config.Mode is ProgressionMode.Free)
            return true;

        // Find all goals that contain this project
        var goalProjects = await ctx.GoalProject
            .Where(gp => gp.ProjectId == projectId)
            .Include(gp => gp.Goal)
            .ToListAsync(token);

        // Orphan projects (not linked to any goal) can be subscribed to freely
        if (goalProjects.Count == 0)
            return true;

        // Filter to goals that are active and not deprecated
        var eligibleGoalIds = goalProjects
            .Where(gp => gp.Goal.Active && !gp.Goal.Deprecated)
            .Select(gp => gp.GoalId)
            .ToList();

        // If no eligible goals exist, cannot subscribe
        if (eligibleGoalIds.Count == 0)
            return false;

        // Check if user is subscribed (Active) to at least one eligible goal
        return await ctx.UserGoals
            .AnyAsync(ug =>
                ug.UserId == userId &&
                eligibleGoalIds.Contains(ug.GoalId) &&
                ug.State == EntityObjectState.Active,
                token);
    }

    public async Task<UserCursus> SubscribeToCursusAsync(Guid userId, Guid cursusId, CancellationToken token = default)
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

    public async Task<UserGoal> SubscribeToGoalAsync(Guid userId, Guid goalId, CancellationToken token = default)
    {
        var userGoal = await ctx.UserGoals
            .Where(ug => ug.GoalId == goalId && ug.UserId == userId)
            .FirstOrDefaultAsync(token);

        // Instance already exists
        if (userGoal is not null)
        {
            if (userGoal.State is not EntityObjectState.Inactive)
                throw new ServiceException(409, "Already subscribed to this goal");
            if (userGoal.State is EntityObjectState.Completed)
                throw new ServiceException(422, "Goal is already completed");

            userGoal.State = EntityObjectState.Active;
            ctx.UserGoals.Update(userGoal);
            await ctx.SaveChangesAsync(token);
            return userGoal;
        }

        // Lets check all the user projects under this goal
        var projectStats = await ctx.GoalProject
            .Where(gp => gp.GoalId == goalId)
            .Select(gp => new
            {
                IsCompleted = ctx.UserProjects.Any(up =>
                    up.ProjectId == gp.ProjectId &&
                    up.Members.Any(m => m.UserId == userId) &&
                    up.State == EntityObjectState.Completed)
            })
            .ToListAsync(token);

        var state = projectStats.Count > 0 && projectStats.All(p => p.IsCompleted)
            ? EntityObjectState.Completed
            : EntityObjectState.Active;

        var result = await ctx.UserGoals.AddAsync(new()
        {
            GoalId = goalId,
            UserId = userId,
            State = state
        }, token);

        await ctx.SaveChangesAsync(token);
        return result.Entity;
    }

    public async Task<UserProject> SubscribeToProjectAsync(Guid userId, Guid projectId, CancellationToken token = default)
    {
        return await ctx.Database.CreateExecutionStrategy().ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);

            // Only match sessions where the user is an actual participant (not a pending invite).
            var up = await ctx.UserProjects
                .Include(up => up.Members)
                .FirstOrDefaultAsync(up => up.ProjectId == projectId && up.Members.Any(m => m.UserId == userId && m.Role != UserProjectRole.Pending), ct);

            if (up is not null)
            {
                if (up.State == EntityObjectState.Active)
                    throw new ServiceException(409, "Already subscribed to this project");

                if (up.State != EntityObjectState.Inactive)
                    throw new ServiceException(400, "Cannot subscribe to this project in its current state");

                up.State = EntityObjectState.Active;
                ctx.UserProjects.Update(up);

                // Reset LeftAt for the rejoining member so future unsubscribes aren't blocked.
                var rejoiningMember = up.Members.FirstOrDefault(m => m.UserId == userId);
                if (rejoiningMember is not null)
                {
                    rejoiningMember.LeftAt = null;
                    ctx.UserProjectMembers.Update(rejoiningMember);
                }

                await ctx.UserProjectTransactions.AddAsync(new()
                {
                    UserId = userId,
                    UserProjectId = up.Id,
                    Type = UserProjectTransactionVariant.StateChangedToActive,
                }, ct);

                await ctx.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return up;
            }

            // Cancel any pending invites this user may have for this project before creating
            // their own session, so they can freely start fresh without accepting/declining.
            var pendingInvites = await ctx.UserProjectMembers
                .Where(m => m.UserId == userId
                    && m.Role == UserProjectRole.Pending
                    && ctx.UserProjects.Any(p => p.Id == m.UserProjectId && p.ProjectId == projectId))
                .ToListAsync(ct);

            if (pendingInvites.Count > 0)
                ctx.UserProjectMembers.RemoveRange(pendingInvites);

            up = new UserProject
            {
                ProjectId = projectId,
                State = EntityObjectState.Active,
                Members = [new() { UserId = userId, Role = UserProjectRole.Leader }]
            };

            await ctx.UserProjects.AddAsync(up, ct);
            await ctx.UserProjectTransactions.AddAsync(new()
            {
                UserId = userId,
                UserProjectId = up.Id,
                Type = UserProjectTransactionVariant.Started,
            }, ct);

            await ctx.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return up;
        }, token);
    }

    public async Task UnsubscribeFromCursusAsync(Guid userId, Guid cursusId, CancellationToken token = default)
    {
        var userCursus = ctx.UserCursi.FirstOrDefault(uc => uc.UserId == userId && uc.CursusId == cursusId);
        if (userCursus is null || userCursus.State is EntityObjectState.Inactive)
            throw new ServiceException(409, "Not subscribed to this cursus.");

        userCursus.State = EntityObjectState.Inactive;
        ctx.UserCursi.Update(userCursus);
        await ctx.SaveChangesAsync(token);
    }

    public async Task UnsubscribeFromGoalAsync(Guid userId, Guid goalId, CancellationToken token = default)
    {
        var goal = await ctx.UserGoals
            .Where(ug => ug.GoalId == goalId && ug.UserId == userId)
            .FirstOrDefaultAsync(token) ?? throw new ServiceException(404, "Not subscribed to this goal");
        if (goal.State is EntityObjectState.Inactive)
            throw new ServiceException(409, "Already unsubscribed from this goal");

        goal.State = EntityObjectState.Inactive;
        ctx.UserGoals.Update(goal);
        await ctx.SaveChangesAsync(token);
    }

    public async Task UnsubscribeFromProjectAsync(Guid userId, Guid projectId, CancellationToken token = default)
    {
        await ctx.Database.CreateExecutionStrategy().ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);

            var up = await ctx.UserProjects
                .Include(up => up.Members)
                .Where(up => up.ProjectId == projectId && up.Members.Any(m => m.UserId == userId))
                .FirstOrDefaultAsync(ct) ?? throw new ServiceException(404, "Not subscribed to this project");

            var member = up.Members.First(m => m.UserId == userId);

            if (member.LeftAt is not null)
                throw new ServiceException(409, "Already left this project session");

            if (member.Role is UserProjectRole.Leader)
            {
                // If other active members exist, the leader must transfer leadership first.
                var hasOtherActiveMembers = up.Members.Any(m =>
                    m.UserId != userId &&
                    m.LeftAt == null &&
                    m.Role is UserProjectRole.Member or UserProjectRole.Leader);

                if (hasOtherActiveMembers)
                    throw new ServiceException(422,
                        "Transfer leadership or remove all members before leaving the session");

                // Leader is the sole remaining member — deactivate the session.
                up.State = EntityObjectState.Inactive;
                member.LeftAt = DateTimeOffset.UtcNow;
                ctx.UserProjects.Update(up);

                // Cancel any orphaned pending invites
                var pendingMembers = up.Members.Where(m => m.Role is UserProjectRole.Pending).ToList();
                foreach (var pending in pendingMembers)
                    ctx.UserProjectMembers.Remove(pending);

                await ctx.UserProjectTransactions.AddAsync(new()
                {
                    UserId = userId,
                    UserProjectId = up.Id,
                    Type = UserProjectTransactionVariant.StateChangedToInActive,
                }, ct);
            }
            else
            {
                // Regular member leaves — preserve history via LeftAt.
                member.LeftAt = DateTimeOffset.UtcNow;
                ctx.UserProjectMembers.Update(member);
                await ctx.UserProjectTransactions.AddAsync(new()
                {
                    UserId = userId,
                    UserProjectId = up.Id,
                    Type = UserProjectTransactionVariant.MemberLeft,
                }, ct);
            }

            await ctx.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }, token);
    }
}