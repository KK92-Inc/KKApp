// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using App.Backend.Domain.Relations;
using App.Backend.Domain.Enums;
using App.Backend.Models.Responses.Entities.Cursi;
using App.Backend.Domain.Entities;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class UserCursusService(DatabaseContext ctx) : BaseService<UserCursus>(ctx), IUserCursusService
{
    private readonly DatabaseContext context = ctx;

    public async Task<(IReadOnlyList<UserCursusGoal> Snapshot, IReadOnlyDictionary<Guid, EntityObjectState> States)> GetTrackAsync(
        Guid userCursusId, Guid userId, CancellationToken token = default)
    {
        var snapshot = await context.UserCursusGoal
            .Where(g => g.UserCursusId == userCursusId)
            .Include(g => g.Goal)
            .ToListAsync(token);

        if (snapshot.Count == 0)
            return ([], new Dictionary<Guid, EntityObjectState>());

        var goalIds = snapshot.Select(n => n.GoalId).ToList();
        var states = await context.UserGoals
            .Where(ug => ug.UserId == userId && goalIds.Contains(ug.GoalId))
            .ToDictionaryAsync(ug => ug.GoalId, ug => ug.State, token);

        return (snapshot, states);
    }

    public UserCursusTrackDO AssembleTrack(
        Cursus cursus,
        IReadOnlyList<UserCursusGoal> snapshot,
        IReadOnlyDictionary<Guid, EntityObjectState> states)
    {
        var completed = states
            .Where(kvp => kvp.Value == EntityObjectState.Completed)
            .Select(kvp => kvp.Key)
            .ToHashSet();

        var unlocked = cursus.Mode switch
        {
            CursusMode.FreeStyle => ComputeFreestyleUnlocked(snapshot, completed),
            CursusMode.Ring => ComputeRingUnlocked(snapshot, completed),
            _ => throw new ArgumentOutOfRangeException(nameof(cursus.Mode))
        };

        return new UserCursusTrackDO
        {
            CursusId = cursus.Id,
            Name = cursus.Name,
            CompletionMode = cursus.Mode,
            Nodes = [.. snapshot.Select(n => new UserCursusTrackNodeDO
            {
                GoalId = n.GoalId,
                Name = n.Goal.Name,
                Slug = n.Goal.Slug,
                ParentGoalId = n.ParentGoalId,
                State = states.TryGetValue(n.GoalId, out var s) ? s : null,
                IsUnlocked = unlocked.Contains(n.GoalId)
            })]
        };
    }

    // FreeStyle: a goal is unlocked when its direct parent is completed (roots always unlocked).
    private static HashSet<Guid> ComputeFreestyleUnlocked(IReadOnlyList<UserCursusGoal> snapshot, HashSet<Guid> completed)
    {
        return [.. snapshot
            .Where(n => n.ParentGoalId is null || completed.Contains(n.ParentGoalId.Value))
            .Select(n => n.GoalId)];
    }

    // Ring: all goals at depth N must be satisfied before depth N+1 unlocks.
    private static HashSet<Guid> ComputeRingUnlocked(IReadOnlyList<UserCursusGoal> snapshot, HashSet<Guid> completed)
    {
        var depths = ComputeDepths(snapshot);
        int maxUnlocked = 0;

        foreach (var depthGroup in snapshot.GroupBy(n => depths[n.GoalId]).OrderBy(g => g.Key))
        {
            maxUnlocked = depthGroup.Key;

            if (!depthGroup.All(n => completed.Contains(n.GoalId)))
                break;

            maxUnlocked = depthGroup.Key + 1;
        }

        return [.. snapshot
            .Where(n => depths[n.GoalId] <= maxUnlocked)
            .Select(n => n.GoalId)];
    }

    // Memoised parent-chain traversal; roots are depth 0.
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

    public async Task<UserCursus?> FindByUserAndCursusAsync(Guid userId, Guid cursusId, CancellationToken token = default)
    {
        return await Query(false).FirstOrDefaultAsync(
            uc => uc.UserId == userId && uc.CursusId == cursusId, token
        );
    }
}
