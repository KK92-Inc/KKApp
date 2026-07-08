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

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class CursusService(DatabaseContext ctx, IGoalService goalService, ILogger<CursusService> log) : BaseService<Cursus>(ctx), ICursusService, ISlugQueryable<Cursus>
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

            // --- PROPAGATE CHANGES TO USER SNAPSHOTS IN A FRONTIER FASHION ---
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
    /// Merges global track changes into every user's snapshot for this cursus.
    ///
    /// Rule: once any goal in a user's snapshot is locked-in (Active/Completed), that
    /// goal's entire subtree - started or not - is frozen exactly as recorded. The
    /// master track can never reach into a branch the user has already committed to.
    /// A master node whose entire purpose is feeding an already-frozen descendant
    /// (i.e. it has no unclaimed content anywhere below it) is pruned rather than
    /// inserted, so a "replacement root" that only exists to host an already-frozen
    /// child never shows up as a dangling extra root.
    ///
    /// Batched across the whole cohort - 3 queries total regardless of how many
    /// students are subscribed, instead of 2 queries per student.
    /// </summary>
    private async Task PropagateTrackChangesToUsersAsync(Guid cursusId, List<CursusGoal> newGlobalGoals, CancellationToken ct)
    {
        var userCursuses = await ctx.UserCursi
            .Where(uc => uc.CursusId == cursusId)
            .Select(uc => new { uc.Id, uc.UserId })
            .ToListAsync(ct);

        if (userCursuses.Count == 0)
            return;

        var userCursusIds = userCursuses.Select(uc => uc.Id).ToList();
        var userIds = userCursuses.Select(uc => uc.UserId).Distinct().ToList();

        // Batch-load every snapshot row and every locked-in goal up front.
        var allSnapshotRows = await ctx.UserCursusGoal
            .Where(ucg => userCursusIds.Contains(ucg.UserCursusId))
            .ToListAsync(ct);

        var lockedInRows = await ctx.UserGoals
            .Where(ug => userIds.Contains(ug.UserId) &&
                (ug.State == EntityObjectState.Active || ug.State == EntityObjectState.Completed))
            .Select(ug => new { ug.UserId, ug.GoalId })
            .ToListAsync(ct);

        var snapshotByUserCursus = allSnapshotRows
            .GroupBy(r => r.UserCursusId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var lockedByUser = lockedInRows
            .GroupBy(r => r.UserId)
            .ToDictionary(g => g.Key, g => g.Select(r => r.GoalId).ToHashSet());

        // Master tree is identical for every user in this cursus - build it once.
        var masterChildrenOf = newGlobalGoals
            .Where(g => g.ParentGoalId is not null)
            .GroupBy(g => g.ParentGoalId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.GoalId).ToList());

        // True if this master goal contributes anything not already frozen for the
        // given user - i.e. it or something below it is genuinely new content.
        bool HasNewContent(Guid goalId, HashSet<Guid> frozen)
        {
            if (frozen.Contains(goalId)) return false;
            if (!masterChildrenOf.TryGetValue(goalId, out var children) || children.Count == 0) return true;
            return children.Any(c => HasNewContent(c, frozen));
        }

        var toRemove = new List<UserCursusGoal>();
        var toAdd = new List<UserCursusGoal>();

        foreach (var uc in userCursuses)
        {
            var oldSnapshot = snapshotByUserCursus.GetValueOrDefault(uc.Id, []);

            if (oldSnapshot.Count == 0)
            {
                // Nothing recorded yet - mirror the master track wholesale.
                toAdd.AddRange(newGlobalGoals.Select(g => new UserCursusGoal
                {
                    UserCursusId = uc.Id,
                    GoalId = g.GoalId,
                    ParentGoalId = g.ParentGoalId,
                    ChoiceGroup = g.ChoiceGroup
                }));
                continue;
            }

            var lockedIn = lockedByUser.GetValueOrDefault(uc.UserId, []);
            var oldById = oldSnapshot.ToDictionary(n => n.GoalId);
            var oldParentOf = oldSnapshot.ToDictionary(n => n.GoalId, n => n.ParentGoalId);
            var oldChildrenOf = oldSnapshot
                .Where(n => n.ParentGoalId is not null)
                .GroupBy(n => n.ParentGoalId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(n => n.GoalId).ToList());

            // Frozen = every locked-in goal, its ancestors (so it stays connected to
            // root), and its full descendant subtree (already-presented next steps).
            var frozen = new HashSet<Guid>();
            foreach (var goalId in lockedIn)
            {
                if (!oldById.ContainsKey(goalId)) continue;
                var current = goalId;
                while (frozen.Add(current) && oldParentOf.TryGetValue(current, out var parent) && parent is Guid p)
                    current = p;
            }

            var frontier = new Queue<Guid>(frozen);
            while (frontier.Count > 0)
            {
                var id = frontier.Dequeue();
                if (!oldChildrenOf.TryGetValue(id, out var children)) continue;
                foreach (var child in children)
                    if (frozen.Add(child))
                        frontier.Enqueue(child);
            }

            var survivingMasterIds = newGlobalGoals
                .Where(g => !frozen.Contains(g.GoalId) && HasNewContent(g.GoalId, frozen))
                .Select(g => g.GoalId)
                .ToHashSet();

            // Old, non-frozen nodes with no place left in the (pruned) master track.
            toRemove.AddRange(oldSnapshot.Where(n =>
                !frozen.Contains(n.GoalId) && !survivingMasterIds.Contains(n.GoalId)));

            foreach (var g in newGlobalGoals)
            {
                if (frozen.Contains(g.GoalId) || !survivingMasterIds.Contains(g.GoalId))
                    continue;

                if (oldById.TryGetValue(g.GoalId, out var existing))
                {
                    // Already tracked from the batch query above - mutating is enough.
                    existing.ParentGoalId = g.ParentGoalId;
                    existing.ChoiceGroup = g.ChoiceGroup;
                }
                else
                {
                    toAdd.Add(new UserCursusGoal
                    {
                        UserCursusId = uc.Id,
                        GoalId = g.GoalId,
                        ParentGoalId = g.ParentGoalId,
                        ChoiceGroup = g.ChoiceGroup
                    });
                }
            }
        }

        if (toRemove.Count > 0)
            ctx.UserCursusGoal.RemoveRange(toRemove);
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