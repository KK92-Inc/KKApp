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
using App.Backend.Domain.Entities.Projects;

namespace App.Backend.Core.Services.Implementation;

public class SubscriptionService(DatabaseContext context, IGitService git, TimeProvider time, IUserCursusService userCursusService, IOptions<SubscriptionOptions> options) : ISubscriptionService
{
    private readonly SubscriptionOptions _config = options.Value;

    public async Task<UserCursus> SubscribeToCursusAsync(Guid userId, Guid cursusId, CancellationToken token = default)
    {
        var existing = await context.UserCursi.FirstOrDefaultAsync(
            uc => uc.UserId == userId && uc.CursusId == cursusId,
        token);

        if (existing is not null)
        {
            CheckLock(existing.UnlocksAt);
            if (existing.State is EntityObjectState.Inactive)
            {
                existing.State = EntityObjectState.Active;
                existing.UnlocksAt = null;

                context.UserCursi.Update(existing);

                await CascadeActivateGoalsAsync(userId, cursusId, token);

                await context.SaveChangesAsync(token);
                return existing;
            }

            throw existing.State switch
            {
                EntityObjectState.Active => new ServiceException("Already subscribed to this cursus."),
                EntityObjectState.Completed => new ServiceException("Cannot resubscribe to a completed cursus."),
                EntityObjectState.Awaiting => new ServiceException("Subscription is awaiting approval."),
                _ => new ServiceException("Invalid subscription state.")
            };
        }

        var result = await context.UserCursi.AddAsync(new()
        {
            CursusId = cursusId,
            UserId = userId
        }, token);

        // 1. Fetch the global track blueprint for this cursus
        var globalTrackNodes = await context.CursusGoal
            .Where(cg => cg.CursusId == cursusId)
            .ToListAsync(token);

        // 2. Map the blueprint directly to the user's personal immutable snapshot
        var userTrackSnapshots = globalTrackNodes.Select(cg => new UserCursusGoal
        {
            UserCursusId = result.Entity.Id,
            GoalId = cg.GoalId,
            ParentGoalId = cg.ParentGoalId,
            ChoiceGroup = cg.ChoiceGroup
        }).ToList();

        // 3. Persist the snapshot alongside the user cursus entity
        await context.UserCursusGoal.AddRangeAsync(userTrackSnapshots, token);

        await context.SaveChangesAsync(token);
        return result.Entity;
    }

    public async Task<UserGoal> SubscribeToGoalAsync(Guid userId, Guid goalId, CancellationToken token = default)
    {
        return await context.Database.CreateExecutionStrategy().ExecuteAsync(async (ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);
            var existing = await context.UserGoals.FirstOrDefaultAsync(
                ug => ug.GoalId == goalId && ug.UserId == userId,
            ct);

            if (existing is not null)
            {
                CheckLock(existing.UnlocksAt);
                ServiceException.ThrowIf(
                    existing.State is not EntityObjectState.Inactive,
                    "Already subscribed to this goal."
                );
            }

            // 1. Hierarchy Restriction Check (Enforced only in Restricted mode)
            if (_config.Mode is ProgressionMode.Restricted)
            {
                var cursiWithGoal = context.CursusGoal
                    .Where(cg => cg.GoalId == goalId)
                    .Select(gp => gp.CursusId);

                if (await cursiWithGoal.AnyAsync(ct))
                {
                    // Fetch all active cursus subscriptions for this user that contain this goal
                    var activeUserCursi = await (from uc in context.UserCursi
                                                 join c in context.Cursi on uc.CursusId equals c.Id
                                                 where uc.UserId == userId &&
                                                       uc.State != EntityObjectState.Inactive &&
                                                       cursiWithGoal.Contains(uc.CursusId)
                                                 select new { UserCursus = uc, Cursus = c })
                                                .ToListAsync(ct);

                    if (activeUserCursi.Count == 0)
                    {
                        throw new ServiceException("This goal is not a standalone goal. Subscribe to a cursus that contains this goal to access it.");
                    }

                    // Check if the goal is unlocked in at least one of the active tracks
                    bool isUnlockedInAnyCursus = false;

                    foreach (var item in activeUserCursi)
                    {
                        // Leverage existing UserCursusService calculations
                        var (snapshot, states) = await userCursusService.GetTrackAsync(item.UserCursus.Id, userId, ct);
                        var track = userCursusService.AssembleTrack(item.Cursus, snapshot, states);

                        var trackNode = track.Nodes.FirstOrDefault(n => n.GoalId == goalId);
                        if (trackNode is not null && trackNode.IsUnlocked)
                        {
                            isUnlockedInAnyCursus = true;
                            break; // Goal is accessible; no need to check other cursuses
                        }
                    }

                    if (!isUnlockedInAnyCursus)
                    {
                        throw new ServiceException("This goal is currently locked. Complete its prerequisites within your cursus to unlock it.");
                    }
                }
            }

            if (existing is not null)
            {
                existing.State = EntityObjectState.Active;
                existing.UnlocksAt = null;

                context.UserGoals.Update(existing);
                await CascadeActivateProjectsAsync(userId, [goalId], ct);

                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return existing;
            }

            // 2. Check goal project composition
            var total = await context.GoalProject.CountAsync(gp => gp.GoalId == goalId, ct);
            var completed = await context.GoalProject
                .Where(gp => gp.GoalId == goalId)
               .CountAsync(gp => context.UserProjects.Any(up =>
                   up.ProjectId == gp.ProjectId &&
                   up.State == EntityObjectState.Completed &&
                   context.Members.Any(m => m.EntityType == MemberEntityType.UserProject &&
                       m.EntityId == up.Id &&
                       m.UserId == userId
                   )),
               ct);

            var userGoal = await context.UserGoals.AddAsync(new()
            {
                GoalId = goalId,
                UserId = userId,
                State = total > 0 && total == completed
                    ? EntityObjectState.Completed
                    : EntityObjectState.Active
            }, ct);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return userGoal.Entity;
        }, token);
    }

    public async Task<UserProject> SubscribeToProjectAsync(Guid userId, Guid projectId, CancellationToken token = default)
    {
        var existing = await context.UserProjects.Where(
            up => up.ProjectId == projectId &&
            context.Members.Any(
                m => m.EntityType == MemberEntityType.UserProject &&
                m.EntityId == up.Id &&
                m.UserId == userId &&
                m.Role != MemberRole.Pending
            )
        ).FirstOrDefaultAsync(token);

        if (existing?.State is EntityObjectState.Completed)
            throw new ServiceException("Cannot resubscribe to a completed project.");

        return await context.Database.CreateExecutionStrategy().ExecuteAsync(async (ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            if (existing is not null)
            {
                CheckLock(existing.UnlocksAt);
                ServiceException.ThrowIf(
                    existing.State is not EntityObjectState.Inactive,
                    "Already subscribed to this project."
                );
            }

            // 1. Hierarchy Restriction check (Enforced only in Restricted mode)
            if (_config.Mode is ProgressionMode.Restricted)
            {
                var goalsWithProject = context.GoalProject
                    .Where(gp => gp.ProjectId == projectId)
                    .Select(gp => gp.GoalId);

                if (await goalsWithProject.AnyAsync(ct))
                {
                    bool subscribed = await context.UserGoals.AnyAsync(
                        ug => ug.UserId == userId &&
                        ug.State != EntityObjectState.Inactive &&
                        goalsWithProject.Contains(ug.GoalId),
                    ct);

                    ServiceException.ThrowIf(!subscribed, "Please subscribe to a goal with this project to access it.");
                }
            }

            if (existing is not null)
            {
                existing.State = EntityObjectState.Active;
                existing.UnlocksAt = null;

                context.UserProjects.Update(existing);

                var rejoiner = await context.Members.FirstOrDefaultAsync(m =>
                    m.EntityType == MemberEntityType.UserProject &&
                    m.EntityId == existing.Id &&
                    m.UserId == userId,
                ct);

                if (rejoiner is not null)
                {
                    rejoiner.LeftAt = null;
                    rejoiner.GitId = existing.GitInfoId;
                    context.Members.Update(rejoiner);
                }

                await context.UserProjectTransactions.AddAsync(new()
                {
                    UserId = userId,
                    UserProjectId = existing.Id,
                    Type = UserProjectTransactionVariant.StateChangedToActive,
                }, ct);

                await git.UnlockAsync(projectId.ToString(), existing.Id.ToString(), ct);
                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return existing;
            }

            // Setup session and Git repository
            var up = new UserProject
            {
                ProjectId = projectId,
                State = EntityObjectState.Active
            };

            var name = up.Id.ToString();
            var owner = projectId.ToString();
            var success = await git.CreateAsync(owner, name, ct);
            ServiceException.ThrowIf(!success, "User repository already exists for project");

            var gitInfo = await context.GitInfo.AddAsync(new Git
            {
                Name = name,
                Owner = owner,
                Ownership = EntityOwnership.User
            }, ct);

            up.GitInfoId = gitInfo.Entity.Id;
            var result = await context.UserProjects.AddAsync(up, ct);
            await context.Members.AddAsync(new Member
            {
                EntityId = up.Id,
                EntityType = MemberEntityType.UserProject,
                UserId = userId,
                GitId = gitInfo.Entity.Id,
                Role = MemberRole.Leader,
            }, ct);

            await context.UserProjectTransactions.AddAsync(new()
            {
                UserId = userId,
                UserProjectId = up.Id,
                Type = UserProjectTransactionVariant.Started,
            }, ct);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return result.Entity;
        }, token);
    }

    // ==============================================================================

    public async Task<UserProject> UnsubscribeFromProjectAsync(Guid userId, Guid projectId, CancellationToken token = default)
    {
        var existing = await context.UserProjects.Where(
            up => up.ProjectId == projectId &&
            context.Members.Any(
                m => m.EntityType == MemberEntityType.UserProject &&
                m.EntityId == up.Id &&
                m.UserId == userId &&
                m.Role != MemberRole.Pending
            )
        ).FirstOrDefaultAsync(token);


        if (existing is null || existing.State is EntityObjectState.Inactive)
            throw new ServiceException("Not currently subscribed to this project.");
        if (existing.State is EntityObjectState.Completed)
            throw new ServiceException("Cannot unsubscribe from a completed project.");

        return await context.Database.CreateExecutionStrategy().ExecuteAsync(async (ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            existing.State = EntityObjectState.Inactive;
            existing.UnlocksAt = time.GetUtcNow().Add(_config.Cooldown);
            context.UserProjects.Update(existing);

            await context.UserProjectTransactions.AddAsync(new()
            {
                UserProjectId = existing.Id,
                Type = UserProjectTransactionVariant.StateChangedToInActive,
                UserId = userId,
            }, ct);

            await git.LockAsync(projectId.ToString(), existing.Id.ToString(), ct);
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return existing;

        }, token);
    }

    public async Task<UserGoal> UnsubscribeFromGoalAsync(Guid userId, Guid goalId, CancellationToken token = default)
    {
        return await context.Database.CreateExecutionStrategy().ExecuteAsync(async (ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            var existing = await context.UserGoals.FirstOrDefaultAsync(
                ug => ug.GoalId == goalId && ug.UserId == userId, ct);

            if (existing is null || existing.State == EntityObjectState.Inactive)
                throw new ServiceException("Not currently subscribed to this goal.");

            existing.State = EntityObjectState.Inactive;
            existing.UnlocksAt = time.GetUtcNow().Add(_config.Cooldown);

            context.UserGoals.Update(existing);

            await CascadeDeactivateProjectsAsync(userId, [goalId], ct);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return existing;
        }, token);
    }

    public async Task<UserCursus> UnsubscribeFromCursusAsync(Guid userId, Guid cursusId, CancellationToken token = default)
    {
        return await context.Database.CreateExecutionStrategy().ExecuteAsync(async (ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            var existing = await context.UserCursi.FirstOrDefaultAsync(
                uc => uc.UserId == userId && uc.CursusId == cursusId, ct);

            if (existing is null || existing.State == EntityObjectState.Inactive)
                throw new ServiceException("Not currently subscribed to this cursus.");

            existing.State = EntityObjectState.Inactive;
            existing.UnlocksAt = time.GetUtcNow().Add(_config.Cooldown);

            context.UserCursi.Update(existing);

            await CascadeDeactivateGoalsAsync(userId, cursusId, ct);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return existing;
        }, token);
    }

    // ==============================================================================

    /// <summary>
    /// Make sure the user isn't trying to resubscribe during an active cooldown.
    /// </summary>
    /// <param name="unlocksAt">When the user can resubscribe.</param>
    /// <exception cref="ServiceException"></exception>
    private void CheckLock(DateTimeOffset? unlocksAt)
    {
        var now = time.GetUtcNow();
        if (unlocksAt.HasValue && now < unlocksAt.Value)
        {
            var remaining = unlocksAt.Value - now;
            var formatted = $"{(int)remaining.TotalHours}h {remaining.Minutes}m";
            throw new ServiceException($"Please wait {formatted} before resubscribing");
        }
    }

    /// <summary>
    /// When a user resubscribes to a cursus, reactivate any goals that are part of that cursus and were inactivated due to the unsubscription.
    /// This ensures that users don't lose progress on goals they were previously subscribed to when they rejoin a cursus.
    /// 
    /// The same applies for projects under those goals.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="reactivatedCursusId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task CascadeActivateGoalsAsync(Guid userId, Guid reactivatedCursusId, CancellationToken ct)
    {
        var inactiveGoals = await context.UserGoals
            .Where(ug => ug.UserId == userId && ug.State == EntityObjectState.Inactive)
            .Where(ug => context.CursusGoal.Any(cg => cg.CursusId == reactivatedCursusId && cg.GoalId == ug.GoalId))
            .ToListAsync(ct);

        if (inactiveGoals.Count == 0) return;

        foreach (var goal in inactiveGoals)
        {
            goal.State = EntityObjectState.Active;
            goal.UnlocksAt = null;
            context.UserGoals.Update(goal);
        }

        var activatedGoalIds = inactiveGoals.Select(g => g.GoalId).ToList();
        await CascadeActivateProjectsAsync(userId, activatedGoalIds, ct);
    }

    /// <summary>
    /// When a user resubscribes to a goal, reactivate any projects that are part of that goal and were inactivated due to the unsubscription.
    /// This ensures that users don't lose progress on projects they were previously subscribed to when they rejoin a goal.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="reactivatedGoalIds"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task CascadeActivateProjectsAsync(Guid userId, List<Guid> reactivatedGoalIds, CancellationToken ct)
    {
        var inactiveProjects = await context.UserProjects
            .Where(up => up.State == EntityObjectState.Inactive)
            .Where(up => context.Members.Any(m => m.EntityType == MemberEntityType.UserProject && m.EntityId == up.Id && m.UserId == userId && m.Role != MemberRole.Pending))
            .Where(up => context.GoalProject.Any(gp => reactivatedGoalIds.Contains(gp.GoalId) && gp.ProjectId == up.ProjectId))
            .ToListAsync(ct);

        if (inactiveProjects.Count == 0) return;

        var transactions = new List<UserProjectTransaction>();
        foreach (var project in inactiveProjects)
        {
            project.State = EntityObjectState.Active;
            project.UnlocksAt = null;
            context.UserProjects.Update(project);

            transactions.Add(new UserProjectTransaction
            {
                UserProjectId = project.Id,
                Type = UserProjectTransactionVariant.StateChangedToActive,
                UserId = userId,
            });
        }

        await context.UserProjectTransactions.AddRangeAsync(transactions, ct);
    }

    /// <summary>
    /// When a user unsubscribes from a cursus, inactivate any goals that are part of that cursus and don't belong to any other active cursus for that user.
    /// This ensures that users lose access to goals that are only available through the cursus they unsubscribed from,
    /// while retaining access to goals that are available through other active cursuses.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="unsubscribedCursusId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task CascadeDeactivateGoalsAsync(Guid userId, Guid unsubscribedCursusId, CancellationToken ct)
    {
        var orphanedGoals = await context.UserGoals
            .Where(ug => ug.UserId == userId && ug.State != EntityObjectState.Inactive)
            .Where(ug => context.CursusGoal.Any(cg => cg.CursusId == unsubscribedCursusId && cg.GoalId == ug.GoalId))
            .Where(ug => !context.CursusGoal.Any(cg =>
                cg.CursusId != unsubscribedCursusId &&
                cg.GoalId == ug.GoalId &&
                context.UserCursi.Any(uc => uc.CursusId == cg.CursusId && uc.UserId == userId && uc.State != EntityObjectState.Inactive)))
            .ToListAsync(ct);

        if (orphanedGoals.Count == 0) return;

        foreach (var goal in orphanedGoals)
        {
            goal.State = EntityObjectState.Inactive;
            goal.UnlocksAt = time.GetUtcNow().Add(_config.Cooldown);
            context.UserGoals.Update(goal);
        }

        var orphanedGoalIds = orphanedGoals.Select(g => g.GoalId).ToList();
        await CascadeDeactivateProjectsAsync(userId, orphanedGoalIds, ct);
    }

    /// <summary>
    /// When a user unsubscribes from a goal, inactivate any projects that are part of that goal and don't belong to any other active goal or cursus for that user.
    /// This ensures that users lose access to projects that are only available through the goal they unsubscribed from,
    /// while retaining access to projects that are available through other active goals.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="unsubscribedGoalIds"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task CascadeDeactivateProjectsAsync(Guid userId, List<Guid> unsubscribedGoalIds, CancellationToken ct)
    {
        var orphanedProjects = await context.UserProjects
            .Where(up => up.State != EntityObjectState.Inactive)
            .Where(up => context.Members.Any(m => m.EntityType == MemberEntityType.UserProject && m.EntityId == up.Id && m.UserId == userId && m.Role != MemberRole.Pending))
            .Where(up => context.GoalProject.Any(gp => unsubscribedGoalIds.Contains(gp.GoalId) && gp.ProjectId == up.ProjectId))
            .Where(up => !context.GoalProject.Any(gp =>
                !unsubscribedGoalIds.Contains(gp.GoalId) &&
                gp.ProjectId == up.ProjectId &&
                (
                    context.UserGoals.Any(ug => ug.GoalId == gp.GoalId && ug.UserId == userId && ug.State != EntityObjectState.Inactive) ||
                    context.CursusGoal.Any(cg => cg.GoalId == gp.GoalId && context.UserCursi.Any(uc => uc.CursusId == cg.CursusId && uc.UserId == userId && uc.State != EntityObjectState.Inactive))
                )))
            .ToListAsync(ct);

        if (orphanedProjects.Count == 0) return;

        var transactions = new List<UserProjectTransaction>();
        foreach (var project in orphanedProjects)
        {
            project.State = EntityObjectState.Inactive;
            project.UnlocksAt = time.GetUtcNow().Add(_config.Cooldown);
            context.UserProjects.Update(project);

            transactions.Add(new UserProjectTransaction
            {
                UserProjectId = project.Id,
                Type = UserProjectTransactionVariant.StateChangedToInActive,
                UserId = userId,
            });
        }

        await context.UserProjectTransactions.AddRangeAsync(transactions, ct);
    }
}