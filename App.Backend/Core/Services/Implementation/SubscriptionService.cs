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

public class SubscriptionService(
    DatabaseContext ctx,
    IOptions<SubscriptionOptions> options
) : ISubscriptionService
{
    private bool IsRestricted => options.Value.Mode is CursusCompletion.Restricted;

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

        // Chain enforcement: if this goal is in a system workspace and belongs to a cursus,
        // the user must be enrolled in at least one. User-owned workspace goals are always free.
        if (IsRestricted)
        {
            var isSystemGoal = await ctx.Goals
            .Where(g => g.Id == goalId)
            .Select(g => g.Workspace.OwnerId == null)
            .FirstOrDefaultAsync(token);

            if (isSystemGoal)
            {
                var parentCursusIds = await ctx.CursusGoal
                    .Where(cg => cg.GoalId == goalId)
                    .Select(cg => cg.CursusId)
                    .ToListAsync(token);

                if (parentCursusIds.Count > 0)
                {
                    var isEnrolled = await ctx.UserCursi.AnyAsync(uc =>
                        uc.UserId == userId &&
                        parentCursusIds.Contains(uc.CursusId) &&
                        uc.State == EntityObjectState.Active,
                        token
                    );

                    if (!isEnrolled)
                        throw new ServiceException(422, "This goal belongs to a cursus — you must be enrolled in the cursus first");
                }
            }
        }

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
        // Chain enforcement: if this project is in a system workspace and belongs to a goal,
        // the user must be subscribed to at least one. User-owned workspace projects are always free.
        if (IsRestricted)
        {
            var isSystemProject = await ctx.Projects
                .Where(p => p.Id == projectId)
                .Select(p => p.Workspace.OwnerId == null)
                .FirstOrDefaultAsync(token);

            if (isSystemProject)
            {
                var parentGoalIds = await ctx.GoalProject
                    .Where(gp => gp.ProjectId == projectId)
                    .Select(gp => gp.GoalId)
                    .ToListAsync(token);

                if (parentGoalIds.Count > 0)
                {
                    var subscribedToAny = await ctx.UserGoals.AnyAsync(
                        ug => ug.UserId == userId
                            && parentGoalIds.Contains(ug.GoalId)
                            && ug.State == EntityObjectState.Active,
                        token
                    );

                    if (!subscribedToAny)
                        throw new ServiceException(422,
                            "This project belongs to a goal — you must be subscribed to the goal first");
                }
            }
        }

        var strategy = ctx.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);

            var up = await ctx.UserProjects
                .FirstOrDefaultAsync(up => up.ProjectId == projectId && up.Members.Any(m => m.UserId == userId), ct);

            if (up is not null)
            {
                if (up.State == EntityObjectState.Active)
                    throw new ServiceException(409, "Already subscribed to this project");

                if (up.State != EntityObjectState.Inactive)
                    throw new ServiceException(400, "Cannot subscribe to this project in its current state");

                up.State = EntityObjectState.Active;
                ctx.UserProjects.Update(up);
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

    public async Task UnsubscribeFromProjectAsync(Guid userId, Guid projectId, CancellationToken token)
    {
        var strategy = ctx.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async (ct) =>
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
                var pendingMembers = up.Members.Where(m => m.Role == UserProjectRole.Pending).ToList();
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
