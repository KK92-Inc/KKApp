// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Relations;
using App.Backend.Core.Services.Persistence.Interface;

// ============================================================================

namespace App.Backend.Core.Services.Persistence.Implementation;

/// <inheritdoc />
public class PersistenceGraphMesher : IPersistenceGraphMesher
{
    /// <inheritdoc />
    public Difference Diff(IReadOnlyList<UserCursusGoal> oldSnapshot, IReadOnlyList<CursusGoal> masterTrack, IReadOnlySet<Guid> lockedInGoalIds)
    {
        if (oldSnapshot.Count == 0)
        {
            return new Difference(
                ToRemove: [],
                ToAdd: [.. masterTrack.Select(g => new Node(g.GoalId, g.ParentGoalId, g.ChoiceGroup))],
                ToRepoint: []);
        }

        var oldById = oldSnapshot.ToDictionary(n => n.GoalId);
        var oldParentOf = oldSnapshot.ToDictionary(n => n.GoalId, n => n.ParentGoalId);
        var oldChildrenOf = oldSnapshot
            .Where(n => n.ParentGoalId is not null)
            .GroupBy(n => n.ParentGoalId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(n => n.GoalId).ToList());

        var masterChildrenOf = masterTrack
            .Where(g => g.ParentGoalId is not null)
            .GroupBy(g => g.ParentGoalId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.GoalId).ToList());

        // Frozen = every locked-in goal, its ancestors (so it stays connected to
        // root), and its full descendant subtree (already-presented next steps).
        var frozen = new HashSet<Guid>();
        foreach (var goalId in lockedInGoalIds)
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

        // True if this master goal contributes anything not already frozen for the
        // given user, i.e. it or something below it is genuinely new content.
        bool HasNewContent(Guid goalId)
        {
            if (frozen.Contains(goalId)) return false;
            if (!masterChildrenOf.TryGetValue(goalId, out var children) || children.Count == 0) return true;
            return children.Any(HasNewContent);
        }

        var survivingMasterIds = masterTrack
            .Where(g => !frozen.Contains(g.GoalId) && HasNewContent(g.GoalId))
            .Select(g => g.GoalId)
            .ToHashSet();

        // Old, non-frozen nodes with no place left in the (pruned) master track.
        var toRemove = oldSnapshot
            .Where(n => !frozen.Contains(n.GoalId) && !survivingMasterIds.Contains(n.GoalId))
            .Select(n => n.GoalId)
            .ToList();

        var toAdd = new List<Node>();
        var toRepoint = new List<Node>();

        foreach (var g in masterTrack)
        {
            if (frozen.Contains(g.GoalId) || !survivingMasterIds.Contains(g.GoalId))
                continue;

            var node = new Node(g.GoalId, g.ParentGoalId, g.ChoiceGroup);
            (oldById.ContainsKey(g.GoalId) ? toRepoint : toAdd).Add(node);
        }

        return new Difference(toRemove, toAdd, toRepoint);
    }
}