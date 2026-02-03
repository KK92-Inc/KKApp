// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class SubscriptionService(DatabaseContext ctx) : ISubscriptionService
{
    public async Task<UserCursus> SubscribeToCursusAsync(Guid userId, Guid cursusId, CancellationToken token)
    {
        // 1. A user can subscribe and unsubscribe to cursi
        // 2. A user cannot subscribe to the same cursus twice
        // 3. Unsubscribing from a cursus does not remove progress on its goals/projects
        // NOTE(W2): Cursi are aggregates of goals, similar to how goals are aggregates of projects
        // furthermore they are all separately subscribable entities and don't rely on each other directly (only for progress tracking)
        // so we can treat them similarly to goals in this regard
        // 4. Cursi have a stored jsonb track representing the user's own path through the cursus

        var userCursus = await ctx.UserCursi
            .Where(uc => uc.CursusId == cursusId && uc.UserId == userId)
            .FirstOrDefaultAsync(token);

        // Instance already exists
        if (userCursus is not null)
        {
            if (userCursus.State is EntityObjectState.Completed)
                throw new ServiceException(422, "Cursus is already completed");
            if (userCursus.State is not EntityObjectState.Inactive)
                throw new ServiceException(409, "Already subscribed to this cursus");

            userCursus.State = EntityObjectState.Active;
            ctx.UserCursi.Update(userCursus);
            await ctx.SaveChangesAsync(token);
            return userCursus;
        }

        var result = await ctx.UserCursi.AddAsync(new()
        {
            CursusId = cursusId,
            UserId = userId,
            State = EntityObjectState.Active
        }, token);

        await ctx.SaveChangesAsync(token);
        return result.Entity;
    }

    public async Task UnsubscribeFromCursusAsync(Guid userId, Guid cursusId, CancellationToken token)
    {
        var cursus = await ctx.UserCursi
            .Where(uc => uc.CursusId == cursusId && uc.UserId == userId)
            .FirstOrDefaultAsync(token) ?? throw new ServiceException(404, "Not subscribed to this cursus");

        if (cursus.State is EntityObjectState.Inactive)
            throw new ServiceException(409, "Already unsubscribed from this cursus");

        cursus.State = EntityObjectState.Inactive;
        ctx.UserCursi.Update(cursus);
        await ctx.SaveChangesAsync(token);
    }

    public async Task<UserGoal> SubscribeToGoalAsync(Guid userId, Guid goalId, CancellationToken token)
    {
        // 1. Goals are project aggregate roots
        // 2. A user can subscribe and unsubscribe to goals
        // 3. A user cannot subscribe to the same goal twice
        // 4. When all projects in the goal are completed, the goal is marked as completed for the user
        // 5. If they are completed before subscribing, the goal is marked as completed immediately
        // 6. Unsubscribing from a goal does not remove progress on its projects

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

    public async Task UnsubscribeFromGoalAsync(Guid userId, Guid goalId, CancellationToken token)
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

    public async Task<UserProject> SubscribeToProjectAsync(Guid userId, Guid projectId, CancellationToken token)
    {
        var up = await ctx.UserProjects
            .FirstOrDefaultAsync(up => up.ProjectId == projectId && up.Members.Any(m => m.UserId == userId), token);

        if (up is not null)
        {
            if (up.State == EntityObjectState.Active)
                throw new ServiceException(409, "Already subscribed to this project");

            if (up.State != EntityObjectState.Inactive)
                throw new ServiceException(400, "Cannot subscribe to this project in its current state");

            up.State = EntityObjectState.Active;
            ctx.UserProjects.Update(up);
            await ctx.UserProjectTransactions.AddAsync(new ()
            {
                UserId = userId,
                UserProjectId = up.Id,
                Type = UserProjectTransactionVariant.StateChangedToActive,
            }, token);

            await ctx.SaveChangesAsync(token);
            return up;
        }

        up = new UserProject
        {
            ProjectId = projectId,
            State = EntityObjectState.Active,
            Members = [new() { UserId = userId, Role = UserProjectRole.Leader }]
        };

        await ctx.UserProjects.AddAsync(up, token);
        await ctx.UserProjectTransactions.AddAsync(new ()
        {
            UserId = userId,
            UserProjectId = up.Id,
            Type = UserProjectTransactionVariant.Started,
        }, token);

        await ctx.SaveChangesAsync(token);
        return up;
    }

    public async Task UnsubscribeFromProjectAsync(Guid userId, Guid projectId, CancellationToken token)
    {
        var up = await ctx.UserProjects
            .Include(up => up.Members)
            .Where(up => up.ProjectId == projectId && up.Members.Any(m => m.UserId == userId))
            .FirstOrDefaultAsync(token) ?? throw new ServiceException(404, "Not subscribed to this project");

        var member = up.Members.First(m => m.UserId == userId);

        // Rule 1: If the user is a leader (and presumably the only one or the session owner), set to inactive.
        if (member.Role is UserProjectRole.Leader)
        {
            up.State = EntityObjectState.Inactive;
            ctx.UserProjects.Update(up);
            await ctx.UserProjectTransactions.AddAsync(new ()
            {
                UserId = userId,
                UserProjectId = up.Id,
                Type = UserProjectTransactionVariant.StateChangedToActive,
            }, token);
            await ctx.SaveChangesAsync(token);
        }

        // Rule 2: If the user is just a member, remove them from the session.
        up.Members.Remove(member);
        ctx.UserProjects.Update(up);
        await ctx.UserProjectTransactions.AddAsync(new ()
        {
            UserId = userId,
            UserProjectId = up.Id,
            Type = UserProjectTransactionVariant.MemberLeft,
        }, token);

        await ctx.SaveChangesAsync(token);
    }
}
