// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Relations;
using App.Backend.Domain.Enums;
using App.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using App.Backend.Core.Query;
using System.Linq.Expressions;
using App.Backend.Models.Responses.Entities.Cursus;
using App.Backend.Models.Responses.Entities.Goals;
using App.Backend.Core.Services.Persistence.Interface;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class CursusService(
    DatabaseContext ctx,
    IGoalService goalService,
    ILogger<CursusService> log,
    IPersistenceGraphMesher mesher
) : BaseService<Cursus>(ctx), ICursusService, ISlugQueryable<Cursus>
{
    public async Task<string?> ValidateTrackAsync(
        IReadOnlyList<(Guid GoalId, Guid? ParentId, Guid? Group)> nodes,
        CancellationToken token = default)
    {
        var allIds = nodes.Select(n => n.GoalId).ToList();
        var distinctIds = allIds.Distinct().ToList();

        if (distinctIds.Count != allIds.Count)
            return "Duplicate goals are not allowed in a track";

        if (!await goalService.ExistsAsync(distinctIds, token))
            return "One or more goal IDs are invalid";

        var parentLookup = nodes.ToDictionary(n => n.GoalId, n => n.ParentId);
        var validated = new HashSet<Guid>();

        foreach (var (GoalId, ParentId, Group) in nodes)
        {
            var current = GoalId;
            var path = new HashSet<Guid>();
            while (parentLookup.TryGetValue(current, out var parentId) && parentId.HasValue)
            {
                if (!parentLookup.ContainsKey(parentId.Value))
                    return $"Parent goal {parentId.Value} is not part of this track";

                if (validated.Contains(current))
                    break;

                if (!path.Add(current))
                    return $"Cyclic dependency detected involving goal {current}";

                current = parentId.Value;
            }

            validated.UnionWith(path);
        }

        var invalidGroup = nodes
            .Where(n => n.Group.HasValue)
            .GroupBy(n => n.Group!.Value)
            .FirstOrDefault(g => g.Select(n => n.ParentId).Distinct().Count() > 1);

        if (invalidGroup is not null)
            return $"All goals in choice group {invalidGroup.Key} must share the same parent";

        return null;
    }

    public CursusTrackDO AssembleTrack(Cursus cursus, IReadOnlyList<CursusGoal> goals)
    {
        var entries = goals.Select(g => (
            Node: new CursusTrackNodeDO { Goal = new GoalLightDO(g.Goal), ChoiceGroup = g.ChoiceGroup },
            g.GoalId,
            g.ParentGoalId
        )).ToList();

        var byId = entries.ToDictionary(e => e.GoalId, e => e.Node);
        var roots = new List<CursusTrackNodeDO>();

        foreach (var (node, _, parentId) in entries)
        {
            if (parentId is not null && byId.TryGetValue(parentId.Value, out var parent))
                parent.Children.Add(node);
            else
                roots.Add(node);
        }

        return new CursusTrackDO
        {
            CursusId = cursus.Id,
            Variant = cursus.Variant,
            CompletionMode = cursus.CompletionMode,
            Nodes = roots
        };
    }

    // ============================================================================
    // CursusService.cs (Modified ReplaceTrackAsync & Helper)
    // ============================================================================

    public async Task<IReadOnlyList<CursusGoal>> ReplaceTrackAsync(
        Guid cursusId,
        IEnumerable<CursusGoal> nodes,
        CancellationToken token = default)
    {
        var strategy = ctx.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);

            var cursus = await FindByIdAsync(cursusId, ct)
                ?? throw new ServiceException(404, "Cursus not found");

            if (cursus.Variant != CursusVariant.Static)
                throw new ServiceException(400, "Track can only be replaced on static cursus types");

            var existing = await ctx.CursusGoal
                .Where(cg => cg.CursusId == cursusId)
                .ToListAsync(ct);

            if (existing.Count > 0)
                ctx.CursusGoal.RemoveRange(existing);

            var nodeList = nodes.Select(n => { n.CursusId = cursusId; return n; }).ToList();
            await ctx.CursusGoal.AddRangeAsync(nodeList, ct);
            await ctx.SaveChangesAsync(ct);

            await PropagateTrackChangesToUsersAsync(cursusId, nodeList, ct);

            await ctx.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            log.LogInformation("Replaced track for cursus {CursusId} and updated user frontiers", cursusId);

            return await ctx.CursusGoal
                .Where(cg => cg.CursusId == cursusId)
                .Include(cg => cg.Goal)
                .ToListAsync(ct);
        }, token);
    }

    /// <summary>
    /// Merges global track changes into every <em>actively subscribed</em> user's
    /// snapshot for this cursus, via <see cref="PersistenceGraphMesher"/>.
    ///
    /// Inactive (unsubscribed) user-cursuses are skipped here on purpose: there is
    /// no student watching that snapshot right now, so re-meshing it would be wasted
    /// work on every future track edit until they come back. Instead it is caught up
    /// in one shot by <see cref="CursusSnapshotTracker.AdvanceTrackAsync"/> when they
    /// resubscribe - see <see cref="SubscriptionService.SubscribeToCursusAsync"/>.
    ///
    /// Batched across the whole active cohort - a small, fixed number of queries
    /// regardless of how many students are subscribed, instead of per-student queries.
    /// </summary>
    private async Task PropagateTrackChangesToUsersAsync(Guid cursusId, List<CursusGoal> newGlobalGoals, CancellationToken ct)
    {
        var userCursuses = await ctx.UserCursi
            .Where(uc => uc.CursusId == cursusId && uc.State != EntityObjectState.Inactive)
            .Select(uc => new { uc.Id, uc.UserId })
            .ToListAsync(ct);

        if (userCursuses.Count == 0)
            return;

        var userCursusIds = userCursuses.Select(uc => uc.Id).ToList();
        var userIds = userCursuses.Select(uc => uc.UserId).Distinct().ToList();

        var allSnapshotRows = await ctx.UserCursusGoal
            .Where(ucg => userCursusIds.Contains(ucg.UserCursusId))
            .ToListAsync(ct);

        // Only the goals that could possibly matter here - anything already in a
        // snapshot, or anything the new master track could introduce - not every
        // goal these users have ever touched across every other cursus they're in.
        var relevantGoalIds = allSnapshotRows.Select(r => r.GoalId)
            .Concat(newGlobalGoals.Select(g => g.GoalId))
            .ToHashSet();

        var lockedInRows = await ctx.UserGoals
            .Where(ug => userIds.Contains(ug.UserId) &&
                relevantGoalIds.Contains(ug.GoalId) &&
                (ug.State == EntityObjectState.Active || ug.State == EntityObjectState.Completed))
            .Select(ug => new { ug.UserId, ug.GoalId })
            .ToListAsync(ct);

        var snapshotByUserCursus = allSnapshotRows
            .GroupBy(r => r.UserCursusId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<UserCursusGoal>)[.. g]);

        var lockedByUser = lockedInRows
            .GroupBy(r => r.UserId)
            .ToDictionary(g => g.Key, g => (IReadOnlySet<Guid>)g.Select(r => r.GoalId).ToHashSet());

        var emptyLockedIn = (IReadOnlySet<Guid>)new HashSet<Guid>();
        var toAdd = new List<UserCursusGoal>();
        var toRemoveKeys = new HashSet<(Guid UserCursusId, Guid GoalId)>();
        var repointByKey = new Dictionary<(Guid UserCursusId, Guid GoalId), Node>();

        foreach (var uc in userCursuses)
        {
            var oldSnapshot = snapshotByUserCursus.GetValueOrDefault(uc.Id, []);
            var lockedIn = lockedByUser.GetValueOrDefault(uc.UserId, emptyLockedIn);
            var diff = mesher.Diff(oldSnapshot, newGlobalGoals, lockedIn);

            foreach (var goalId in diff.ToRemove)
                toRemoveKeys.Add((uc.Id, goalId));

            toAdd.AddRange(diff.ToAdd.Select(n => new UserCursusGoal
            {
                UserCursusId = uc.Id,
                GoalId = n.GoalId,
                ParentGoalId = n.ParentGoalId,
                ChoiceGroup = n.ChoiceGroup
            }));

            foreach (var n in diff.ToRepoint)
                repointByKey[(uc.Id, n.GoalId)] = n;
        }

        if (repointByKey.Count > 0)
        {
            foreach (var row in allSnapshotRows)
            {
                if (!repointByKey.TryGetValue((row.UserCursusId, row.GoalId), out var n))
                    continue;

                row.ParentGoalId = n.ParentGoalId;
                row.ChoiceGroup = n.ChoiceGroup;
            }
        }

        if (toRemoveKeys.Count > 0)
        {
            var rowsToRemove = allSnapshotRows
                .Where(r => toRemoveKeys.Contains((r.UserCursusId, r.GoalId)))
                .ToList();
            ctx.UserCursusGoal.RemoveRange(rowsToRemove);
        }

        if (toAdd.Count > 0)
            await ctx.UserCursusGoal.AddRangeAsync(toAdd, ct);
    }

    public async Task<IReadOnlyList<CursusGoal>> GetTrackAsync(Guid cursusId, CancellationToken token = default)
    {
        return await ctx.CursusGoal
            .Where(cg => cg.CursusId == cursusId)
            .Include(cg => cg.Goal)
            .ToListAsync(token);
    }

    public async Task<Cursus?> FindBySlugAsync(string slug, CancellationToken token = default)
    {
        return await ctx.Cursi.FirstOrDefaultAsync(g => g.Slug == slug, token);
    }
}