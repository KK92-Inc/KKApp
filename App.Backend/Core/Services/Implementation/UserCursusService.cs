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

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class UserCursusService(DatabaseContext ctx) : BaseService<UserCursus>(ctx), IUserCursusService
{
    public Task AdvanceTrackAsync(Guid userId, Guid userCursusId, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<UserCursus?> FindByUserAndCursusAsync(Guid userId, Guid cursusId, CancellationToken token = default)
    {
        return await Query(false).FirstOrDefaultAsync(
            uc => uc.UserId == userId && uc.CursusId == cursusId, token
        );
    }

    /// <summary>
    /// Returns the user's frozen snapshot of a cursus track, with goal details
    /// included. This is the source of truth for the user's personal track —
    /// it reflects the state of the cursus at enrollment time, not today.
    /// </summary>
    public async Task<IReadOnlyList<UserCursusGoal>> GetSnapshotAsync(Guid userCursusId, CancellationToken token = default)
    {
        return await ctx.UserCursusGoal
            .Where(ucg => ucg.UserCursusId == userCursusId)
            .Include(ucg => ucg.Goal)
            .ToListAsync(token);
    }

    /// <summary>
    /// Returns the current goal states for a user, scoped to the goal IDs
    /// present in their snapshot. Uses a projected join to avoid loading
    /// full goal entities.
    /// </summary>
    public async Task<IReadOnlyDictionary<Guid, EntityObjectState>> GetSnapshotStatesAsync(
        Guid userId,
        IEnumerable<Guid> goalIds,
        CancellationToken token = default)
    {
        var goalIdSet = goalIds.ToHashSet();

        return await ctx.UserGoals
            .Where(ug => ug.UserId == userId && goalIdSet.Contains(ug.GoalId))
            .ToDictionaryAsync(ug => ug.GoalId, ug => ug.State, token);
    }
}
