// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Core.Services.Options;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Relations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class SubscriptionService(DatabaseContext ctx, IOptions<SubscriptionOptions> options) : ISubscriptionService
{
    private readonly SubscriptionOptions config = options.Value;

    // -------------------------------------------------------------------------
    // Can-subscribe checks
    // -------------------------------------------------------------------------

    public async Task<bool> CanSubscribeToCursusAsync(Guid userId, Guid cursusId, CancellationToken token = default)
    {
        if (config.Mode is ProgressionMode.Free)
            return true;

        return true;
    }

    public async Task<bool> CanSubscribeToGoalAsync(Guid userId, Guid goalId, CancellationToken token = default)
    {
        if (config.Mode is ProgressionMode.Free)
            return true;

        var userGoal = await ctx.UserGoals
            .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GoalId == goalId, token);

        if (userGoal?.State is EntityObjectState.Awaiting or EntityObjectState.Active or EntityObjectState.Completed)
            return false;

        var cursusGoals = await ctx.CursusGoal
            .Where(cg => cg.GoalId == goalId)
            .ToListAsync(token);

        if (cursusGoals.Count == 0)
            return true;

        var enrolledCursusIds = await ctx.UserCursi
            .Where(uc => uc.UserId == userId
                      && (uc.State == EntityObjectState.Active || uc.State == EntityObjectState.Awaiting))
            .Select(uc => uc.CursusId)
            .ToListAsync(token);

        var relevantCursusGoals = cursusGoals
            .Where(cg => enrolledCursusIds.Contains(cg.CursusId))
            .ToList();

        if (relevantCursusGoals.Count == 0)
            return false;

        foreach (var cursusGoal in relevantCursusGoals)
        {
            if (await CanAdvanceToGoalInCursusAsync(userId, cursusGoal, token))
                return true;
        }

        return false;
    }

    private async Task<bool> CanAdvanceToGoalInCursusAsync(Guid userId, CursusGoal cursusGoal, CancellationToken token = default)
    {
        if (cursusGoal.ParentGoalId.HasValue)
        {
            var parentUserGoal = await ctx.UserGoals
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GoalId == cursusGoal.ParentGoalId.Value, token);

            if (parentUserGoal?.State is not EntityObjectState.Completed)
                return false;
        }

        if (cursusGoal.ChoiceGroup.HasValue)
        {
            var siblingGoalIds = await ctx.CursusGoal
                .Where(cg =>
                    cg.CursusId == cursusGoal.CursusId &&
                    cg.ChoiceGroup == cursusGoal.ChoiceGroup &&
                    cg.GoalId != cursusGoal.GoalId)
                .Select(cg => cg.GoalId)
                .ToListAsync(token);

            var hasChosenSibling = await ctx.UserGoals
                .AnyAsync(ug =>
                    ug.UserId == userId &&
                    siblingGoalIds.Contains(ug.GoalId) &&
                    (ug.State == EntityObjectState.Active ||
                     ug.State == EntityObjectState.Awaiting ||
                     ug.State == EntityObjectState.Completed),
                    token);

            if (hasChosenSibling)
                return false;
        }

        return true;
    }

    public async Task<bool> CanSubscribeToProjectAsync(Guid userId, Guid projectId, CancellationToken token = default)
    {
        if (config.Mode is ProgressionMode.Free)
            return true;

        var goalProjects = await ctx.GoalProject
            .Where(gp => gp.ProjectId == projectId)
            .Include(gp => gp.Goal)
            .ToListAsync(token);

        if (goalProjects.Count == 0)
            return true;

        var eligibleGoalIds = goalProjects
            .Where(gp => gp.Goal.Active && !gp.Goal.Deprecated)
            .Select(gp => gp.GoalId)
            .ToList();

        if (eligibleGoalIds.Count == 0)
            return false;

        return await ctx.UserGoals
            .AnyAsync(ug =>
                ug.UserId == userId &&
                eligibleGoalIds.Contains(ug.GoalId) &&
                ug.State == EntityObjectState.Active,
                token);
    }

    // -------------------------------------------------------------------------
    // Subscribe
    // -------------------------------------------------------------------------

    public async Task<UserCursus> SubscribeToCursusAsync(Guid userId, Guid cursusId, CancellationToken token = default)
    {
        var userCursus = await ctx.UserCursi
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CursusId == cursusId, token);

        if (userCursus is not null)
        {
            if (userCursus.State is EntityObjectState.Active)
                throw new ServiceException("Already subscribed to this cursus.");
            if (userCursus.State is EntityObjectState.Completed)
                throw new ServiceException("Cannot resubscribe to a completed cursus.");
            if (userCursus.State is EntityObjectState.Awaiting)
                throw new ServiceException("Subscription is awaiting approval.");

            userCursus.State = EntityObjectState.Active;
            ctx.UserCursi.Update(userCursus);
            await ctx.SaveChangesAsync(token);
            return userCursus;
        }

        var result = await ctx.UserCursi.AddAsync(new() { CursusId = cursusId, UserId = userId }, token);
        await ctx.SaveChangesAsync(token);
        return result.Entity;
    }

    public async Task<UserGoal> SubscribeToGoalAsync(Guid userId, Guid goalId, CancellationToken token = default)
    {
        var userGoal = await ctx.UserGoals
            .Where(ug => ug.GoalId == goalId && ug.UserId == userId)
            .FirstOrDefaultAsync(token);

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

        // Check completion state across all projects under this goal.
        // Members now live in tbl_members, so the subquery joins through there
        // instead of the old navigation property.
        var projectStats = await ctx.GoalProject
            .Where(gp => gp.GoalId == goalId)
            .Select(gp => new
            {
                IsCompleted = ctx.UserProjects.Any(up =>
                    up.ProjectId == gp.ProjectId &&
                    up.State == EntityObjectState.Completed &&
                    ctx.Members.Any(m =>
                        m.EntityType == MemberEntityType.UserProject &&
                        m.EntityId == up.Id &&
                        m.UserId == userId))
            })
            .ToListAsync(token);

        var state = projectStats.Count > 0 && projectStats.All(p => p.IsCompleted)
            ? EntityObjectState.Completed
            : EntityObjectState.Active;

        var result = await ctx.UserGoals.AddAsync(new()
        {
            GoalId = goalId,
            UserId = userId,
            State = state,
        }, token);

        await ctx.SaveChangesAsync(token);
        return result.Entity;
    }

    public async Task<UserProject> SubscribeToProjectAsync(Guid userId, Guid projectId, CancellationToken token = default)
    {
        return await ctx.Database.CreateExecutionStrategy().ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);

            // Find an existing session this user is a non-pending participant of.
            // Can no longer use Include(up => up.Members) — join through tbl_members instead.
            var up = await ctx.UserProjects
                .Where(up =>
                    up.ProjectId == projectId &&
                    ctx.Members.Any(m =>
                        m.EntityType == MemberEntityType.UserProject &&
                        m.EntityId == up.Id &&
                        m.UserId == userId &&
                        m.Role != MemberRole.Pending))
                .FirstOrDefaultAsync(ct);

            if (up is not null)
            {
                if (up.State == EntityObjectState.Active)
                    throw new ServiceException(409, "Already subscribed to this project");
                if (up.State != EntityObjectState.Inactive)
                    throw new ServiceException(400, "Cannot subscribe to this project in its current state");

                // Reactivate the session
                up.State = EntityObjectState.Active;
                ctx.UserProjects.Update(up);

                // Clear LeftAt so future unsubscribes aren't blocked
                var rejoiningMember = await ctx.Members
                    .FirstOrDefaultAsync(m =>
                        m.EntityType == MemberEntityType.UserProject &&
                        m.EntityId == up.Id &&
                        m.UserId == userId, ct);

                if (rejoiningMember is not null)
                {
                    rejoiningMember.LeftAt = null;
                    ctx.Members.Update(rejoiningMember);
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

            // Cancel any pending invites for this project before starting a fresh session,
            // so the user isn't stuck accepting/declining a stale invite.
            var pendingInvites = await ctx.Members
                .Where(m =>
                    m.EntityType == MemberEntityType.UserProject &&
                    m.UserId == userId &&
                    m.Role == MemberRole.Pending &&
                    ctx.UserProjects.Any(p => p.Id == m.EntityId && p.ProjectId == projectId))
                .ToListAsync(ct);

            if (pendingInvites.Count > 0)
                ctx.Members.RemoveRange(pendingInvites);

            // Create the session — then add the leader membership separately
            // since Members is no longer a navigation collection on UserProject.
            up = new UserProject
            {
                ProjectId = projectId,
                State = EntityObjectState.Active,
            };
            await ctx.UserProjects.AddAsync(up, ct);

            // up.Id is generated by BaseEntity before SaveChanges, so it's safe to use here
            await ctx.Members.AddAsync(new Member
            {
                EntityType = MemberEntityType.UserProject,
                EntityId = up.Id,
                GitId = up.GitInfoId,   // null until a git repo is attached
                UserId = userId,
                Role = MemberRole.Leader,
            }, ct);

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

    // -------------------------------------------------------------------------
    // Unsubscribe
    // -------------------------------------------------------------------------

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
            .FirstOrDefaultAsync(token)
            ?? throw new ServiceException(404, "Not subscribed to this goal");

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

            // Find the session this user belongs to
            var up = await ctx.UserProjects
                .Where(up =>
                    up.ProjectId == projectId &&
                    ctx.Members.Any(m =>
                        m.EntityType == MemberEntityType.UserProject &&
                        m.EntityId == up.Id &&
                        m.UserId == userId))
                .FirstOrDefaultAsync(ct)
                ?? throw new ServiceException(404, "Not subscribed to this project");

            var members = await ctx.Members
                .Where(m => m.EntityType == MemberEntityType.UserProject
                        && m.EntityId == up.Id)
                .ToListAsync(ct);

            var member = members.First(m => m.UserId == userId);
            if (member.LeftAt is not null)
                throw new ServiceException(409, "Already left this project session");

            if (member.Role is MemberRole.Leader)
            {
                var hasOtherActiveMembers = members.Any(m =>
                    m.UserId != userId &&
                    m.LeftAt == null &&
                    m.Role is MemberRole.Member or MemberRole.Leader);

                if (hasOtherActiveMembers)
                    throw new ServiceException(422,
                        "Transfer leadership or remove all members before leaving the session");

                // Sole remaining leader — deactivate session and clean up pending invites
                up.State = EntityObjectState.Inactive;
                member.LeftAt = DateTimeOffset.UtcNow;
                ctx.UserProjects.Update(up);
                ctx.Members.Update(member);

                var pendingMembers = members.Where(m => m.Role is MemberRole.Pending).ToList();
                if (pendingMembers.Count > 0)
                    ctx.Members.RemoveRange(pendingMembers);

                await ctx.UserProjectTransactions.AddAsync(new()
                {
                    UserId = userId,
                    UserProjectId = up.Id,
                    Type = UserProjectTransactionVariant.StateChangedToInActive,
                }, ct);
            }
            else
            {
                member.LeftAt = DateTimeOffset.UtcNow;
                ctx.Members.Update(member);

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