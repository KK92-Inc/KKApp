// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Relations;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursus;

/// <summary>
/// The full resolved track for a user's cursus enrollment.
/// </summary>
public record UserCursusTrackDO(
    Guid CursusId,
    string Name,
    CompletionMode CompletionMode,
    IEnumerable<UserCursusTrackNodeDO> Nodes)
{
    /// <summary>
    /// Builds the track from a frozen snapshot and the user's current goal states.
    /// No further DB calls are made here — everything is computed in memory from
    /// the data already loaded by the service layer.
    /// </summary>
    public static UserCursusTrackDO From(
        Domain.Entities.Cursus cursus,
        IReadOnlyList<UserCursusGoal> snapshot,
        IReadOnlyDictionary<Guid, EntityObjectState> userStates)
    {
        var completedGoals = userStates
            .Where(kvp => kvp.Value == EntityObjectState.Completed)
            .Select(kvp => kvp.Key)
            .ToHashSet();

        var unlocked = cursus.CompletionMode switch
        {
            CompletionMode.FreeStyle => ComputeFreestyleUnlocked(snapshot, completedGoals),
            CompletionMode.Ring => ComputeRingUnlocked(snapshot, completedGoals),
            _ => throw new ArgumentOutOfRangeException(nameof(cursus.CompletionMode))
        };

        var nodes = snapshot.Select(n => new UserCursusTrackNodeDO(
            GoalId: n.GoalId,
            Name: n.Goal.Name,
            Slug: n.Goal.Slug,
            ParentGoalId: n.ParentGoalId,
            ChoiceGroup: n.ChoiceGroup,
            State: userStates.TryGetValue(n.GoalId, out var s) ? s : null,
            IsUnlocked: unlocked.Contains(n.GoalId)
        ));

        return new UserCursusTrackDO(cursus.Id, cursus.Name, cursus.CompletionMode, nodes);
    }

    // -------------------------------------------------------------------------
    // FreeStyle: a goal is unlocked when its direct parent is completed
    //   (roots are always unlocked). Choice-group mutual exclusion is enforced
    //   at subscription time — here we simply show all reachable goals.
    // -------------------------------------------------------------------------

    private static HashSet<Guid> ComputeFreestyleUnlocked(
        IReadOnlyList<UserCursusGoal> snapshot,
        HashSet<Guid> completed)
    {
        return [.. snapshot
            .Where(n => n.ParentGoalId is null || completed.Contains(n.ParentGoalId.Value))
            .Select(n => n.GoalId)];
    }

    // -------------------------------------------------------------------------
    // Ring: goals are grouped by depth. All goals at depth N must be satisfied
    //   (required ones completed, each choice group satisfied by at least one)
    //   before depth N+1 becomes accessible. Goals at the next incomplete depth
    //   are shown as unlocked so the user knows what's up next.
    // -------------------------------------------------------------------------

    private static HashSet<Guid> ComputeRingUnlocked(
        IReadOnlyList<UserCursusGoal> snapshot,
        HashSet<Guid> completedGoals)
    {
        var depths = ComputeDepths(snapshot);
        var byDepth = snapshot
            .GroupBy(n => depths[n.GoalId])
            .OrderBy(g => g.Key)
            .ToList();

        // Walk depth by depth; stop at the first unsatisfied level.
        // Goals at that level are still unlocked (visible / subscribable),
        // but nothing deeper opens until it's done.
        int maxUnlocked = 0;
        foreach (var depthGroup in byDepth)
        {
            maxUnlocked = depthGroup.Key;

            var requiredDone = depthGroup
                .Where(n => n.ChoiceGroup is null)
                .All(n => completedGoals.Contains(n.GoalId));

            var choicesDone = depthGroup
                .Where(n => n.ChoiceGroup is not null)
                .GroupBy(n => n.ChoiceGroup)
                .All(g => g.Any(n => completedGoals.Contains(n.GoalId)));

            if (!requiredDone || !choicesDone)
                break; // This depth unlocks but doesn't advance further

            maxUnlocked = depthGroup.Key + 1; // Advance the frontier
        }

        return snapshot
            .Where(n => depths[n.GoalId] <= maxUnlocked)
            .Select(n => n.GoalId)
            .ToHashSet();
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Computes the depth of every node via memoised parent-chain traversal.
    /// Roots (ParentGoalId == null) are depth 0.
    /// </summary>
    private static Dictionary<Guid, int> ComputeDepths(IReadOnlyList<UserCursusGoal> snapshot)
    {
        var parentOf = snapshot.ToDictionary(n => n.GoalId, n => n.ParentGoalId);
        var cache = new Dictionary<Guid, int>(snapshot.Count);

        int Depth(Guid id)
        {
            if (cache.TryGetValue(id, out var d)) return d;
            if (!parentOf.TryGetValue(id, out var parent) || parent is null)
                return cache[id] = 0;
            return cache[id] = 1 + Depth(parent.Value);
        }

        foreach (var n in snapshot)
            Depth(n.GoalId);

        return cache;
    }
}
