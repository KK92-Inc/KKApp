// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Services.Implementation;
using App.Backend.Core.Services.Persistence.Interface;
using App.Backend.Database;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Relations;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Core.Services.Persistence.Implementation;

public class CursusSnapshotTracker(DatabaseContext ctx, IPersistenceGraphMesher mesher) : ICursusSnapshotTracker
{
    public async Task AdvanceTrackAsync(Guid userId, Guid cursusId, Guid userCursusId, CancellationToken token = default)
    {
        var masterTrack = await ctx.CursusGoal
            .Where(cg => cg.CursusId == cursusId)
            .ToListAsync(token);

        var oldSnapshot = await ctx.UserCursusGoal
            .Where(n => n.UserCursusId == userCursusId)
            .ToListAsync(token);

        if (masterTrack.Count == 0 && oldSnapshot.Count == 0)
            return;

        var relevantGoalIds = oldSnapshot.Select(n => n.GoalId)
            .Concat(masterTrack.Select(g => g.GoalId))
            .ToHashSet();

        var lockedIn = await ctx.UserGoals
            .Where(ug => ug.UserId == userId &&
                relevantGoalIds.Contains(ug.GoalId) &&
                (ug.State == EntityObjectState.Active || ug.State == EntityObjectState.Completed))
            .Select(ug => ug.GoalId)
            .ToListAsync(token);

        var diff = mesher.Diff(oldSnapshot, masterTrack, lockedIn.ToHashSet());
        if (diff.IsEmpty)
            return;

        if (diff.ToRemove.Count > 0)
        {
            var removeSet = diff.ToRemove.ToHashSet();
            ctx.UserCursusGoal.RemoveRange(oldSnapshot.Where(n => removeSet.Contains(n.GoalId)));
        }

        if (diff.ToRepoint.Count > 0)
        {
            var oldById = oldSnapshot.ToDictionary(n => n.GoalId);
            foreach (var n in diff.ToRepoint)
            {
                var row = oldById[n.GoalId];
                row.ParentGoalId = n.ParentGoalId;
                row.ChoiceGroup = n.ChoiceGroup;
            }
        }

        if (diff.ToAdd.Count > 0)
        {
            await ctx.UserCursusGoal.AddRangeAsync(diff.ToAdd.Select(n => new UserCursusGoal
            {
                UserCursusId = userCursusId,
                GoalId = n.GoalId,
                ParentGoalId = n.ParentGoalId,
                ChoiceGroup = n.ChoiceGroup
            }), token);
        }

        await ctx.SaveChangesAsync(token);
    }
}