// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Services.Interface;
using App.Backend.Database;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Relations;
using Microsoft.EntityFrameworkCore;

namespace App.Backend.Core.Services.Implementation;

// ============================================================================

public class CursusSnapshotTracker(DatabaseContext context) : ICursusSnapshotTracker
{
    public async Task AdvanceTrackAsync(Guid userId, Guid cursusId, Guid userCursusId, CancellationToken token = default)
    {
        var cursusMode = await context.Cursi
            .Where(c => c.Id == cursusId)
            .Select(c => c.CompletionMode)
            .FirstOrDefaultAsync(token);

        bool changesMade;
        do
        {
            changesMade = false;

            var currentSnapshots = await context.UserCursusGoal.Where(u => u.UserCursusId == userCursusId).ToListAsync(token);
            var snapshottedGoalIds = currentSnapshots.Select(s => s.GoalId).ToHashSet();

            var completedGoals = await context.UserGoals
                .Where(ug => ug.UserId == userId && ug.State == EntityObjectState.Completed)
                .Select(ug => ug.GoalId)
                .ToHashSetAsync(token);

            var liveTrack = await context.CursusGoal.Where(cg => cg.CursusId == cursusId).ToListAsync(token);
            var newlyUnlockedLiveGoals = new List<CursusGoal>();

            if (cursusMode is CompletionMode.FreeStyle)
            {
                newlyUnlockedLiveGoals = [.. liveTrack.Where(live =>
                    !snapshottedGoalIds.Contains(live.GoalId) &&
                    (live.ParentGoalId == null || 
                    (snapshottedGoalIds.Contains(live.ParentGoalId.Value) && completedGoals.Contains(live.ParentGoalId.Value)))
                )];
            }
            else if (cursusMode is CompletionMode.Ring)
            {
                var liveDepths = ComputeDepths(liveTrack);
                var snapshotDepths = ComputeSnapshotDepths(currentSnapshots);
                
                int maxSatisfiedDepth = -1;
                
                if (currentSnapshots.Count > 0)
                {
                    var groupedByDepth = currentSnapshots.GroupBy(s => snapshotDepths[s.GoalId]).OrderBy(g => g.Key).ToList();
                    foreach (var depthGroup in groupedByDepth)
                    {
                        var requiredDone = depthGroup.Where(n => n.ChoiceGroup == null).All(n => completedGoals.Contains(n.GoalId));
                        var choicesDone = depthGroup.Where(n => n.ChoiceGroup != null)
                            .GroupBy(n => n.ChoiceGroup)
                            .All(group => group.Any(n => completedGoals.Contains(n.GoalId)));

                        if (requiredDone && choicesDone) maxSatisfiedDepth = depthGroup.Key;
                        else break;
                    }
                }

                int targetLiveDepth = maxSatisfiedDepth + 1;
                newlyUnlockedLiveGoals = liveTrack.Where(live =>
                    !snapshottedGoalIds.Contains(live.GoalId) &&
                    liveDepths.TryGetValue(live.GoalId, out var depth) && depth == targetLiveDepth
                ).ToList();
            }

            if (newlyUnlockedLiveGoals.Count > 0)
            {
                var newSnapshotEntries = newlyUnlockedLiveGoals.Select(live => new UserCursusGoal
                {
                    UserCursusId = userCursusId,
                    GoalId = live.GoalId,
                    ParentGoalId = live.ParentGoalId,
                    ChoiceGroup = live.ChoiceGroup
                }).ToList();

                context.UserCursusGoal.AddRange(newSnapshotEntries);
                await context.SaveChangesAsync(token);
                
                // If any newly unlocked goal was already completed years ago, loop again to recursively unlock the next tier!
                changesMade = newSnapshotEntries.Any(entry => completedGoals.Contains(entry.GoalId)); 
            }

        } while (changesMade);
    }

    private static Dictionary<Guid, int> ComputeDepths(List<CursusGoal> track)
    {
        var parentMap = track.ToDictionary(n => n.GoalId, n => n.ParentGoalId);
        var cache = new Dictionary<Guid, int>();

        int Depth(Guid id)
        {
            if (cache.TryGetValue(id, out var depth)) return depth;
            if (!parentMap.TryGetValue(id, out var parent) || parent is null) return cache[id] = 0;
            return cache[id] = 1 + Depth(parent.Value);
        }

        foreach (var node in track) Depth(node.GoalId);
        return cache;
    }

    private static Dictionary<Guid, int> ComputeSnapshotDepths(List<UserCursusGoal> track)
    {
        var parentMap = track.ToDictionary(n => n.GoalId, n => n.ParentGoalId);
        var cache = new Dictionary<Guid, int>();

        int Depth(Guid id)
        {
            if (cache.TryGetValue(id, out var depth)) return depth;
            if (!parentMap.TryGetValue(id, out var parent) || parent is null) return cache[id] = 0;
            return cache[id] = 1 + Depth(parent.Value);
        }

        foreach (var node in track) Depth(node.GoalId);
        return cache;
    }
}