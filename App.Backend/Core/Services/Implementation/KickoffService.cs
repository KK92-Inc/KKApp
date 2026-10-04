// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Query;
using App.Backend.Core.Services.Interface;
using App.Backend.Database;
using App.Backend.Database.Extensions;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class KickoffService(DatabaseContext ctx, TimeProvider time) : BaseService<Kickoff>(ctx), IKickoffService
{
    private readonly DatabaseContext ctx = ctx;

    #region Queries

    public async Task<IReadOnlyDictionary<Guid, int>> CountMembersAsync(IReadOnlyCollection<Guid> kickoffIds, CancellationToken token = default)
    {
        if (kickoffIds.Count == 0)
            return new Dictionary<Guid, int>();

        var ids = kickoffIds.ToList();
        var counts = await ctx.Users
            .AsNoTracking()
            .Where(u => u.KickoffId != null && ids.Contains(u.KickoffId.Value))
            .GroupBy(u => u.KickoffId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToListAsync(token);

        return counts.ToDictionary(c => c.Id!.Value, c => c.Count);
    }

    public async Task<Kickoff?> FindByUserAsync(Guid userId, CancellationToken token = default)
    {
        return await ctx.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.Kickoff)
            .FirstOrDefaultAsync(token);
    }

    public async Task<PaginatedList<User>> GetUsersAsync(
        Guid kickoffId,
        UserRole? role,
        ISorting sorting,
        IPagination pagination,
        CancellationToken token = default
    )
    {
        var query = ctx.Users.AsNoTracking().Where(u => u.KickoffId == kickoffId);
        if (role.HasValue)
            query = query.Where(u => u.Role == role.Value);

        return await query.Sort(sorting).PaginateAsync(pagination, token);
    }

    public async Task<IReadOnlyList<Guid>> GetUserIdsAsync(Guid kickoffId, UserRole role, CancellationToken token = default)
    {
        return await ctx.Users
            .AsNoTracking()
            .Where(u => u.KickoffId == kickoffId && u.Role == role)
            .Select(u => u.Id)
            .ToListAsync(token);
    }

    #endregion

    #region Kickoff

    public override Task UpdateAsync(Kickoff entity, CancellationToken token = default)
    {
        return ctx.WithAdvisoryLockAsync($"kickoff:{entity.Id}", async ct =>
        {
            // Compare against what is persisted, not what the caller handed us. We hold the lock,
            // so neither the members nor the start can change underneath these checks.
            var persisted = await ctx.Kickoffs
                .AsNoTracking()
                .Where(k => k.Id == entity.Id)
                .Select(k => new { k.StartsAt, k.StartedAt })
                .FirstOrDefaultAsync(ct)
                ?? throw new ServiceException(404, "Kickoff not found.");

            var members = await ctx.Users.CountAsync(u => u.KickoffId == entity.Id, ct);
            ServiceException.ThrowIf(entity.Capacity < members, 409,
                $"The capacity can't be lower than the current amount of members ({members}).");

            ServiceException.ThrowIf(persisted.StartedAt is not null && entity.StartsAt != persisted.StartsAt, 409,
                "This kickoff has already started, its date can no longer change.");

            entity.StartedAt = persisted.StartedAt; // Owned by the kickoff job, never by a caller.
            await base.UpdateAsync(entity, ct);
        }, token);
    }

    public override async Task DeleteAsync(Kickoff entity, CancellationToken token = default)
    {
        await ctx.WithAdvisoryLockAsync($"kickoff:{entity.Id}", async ct =>
        {
            var members = await ctx.Users.CountAsync(u => u.KickoffId == entity.Id, ct);
            ServiceException.ThrowIf(members > 0, 409, $"This kickoff still has {members} member(s), remove them first.");

            ctx.Kickoffs.Remove(entity);
            await ctx.SaveChangesAsync(ct);
        }, token);
    }

    public Task<bool> EnsureStartedAsync(Guid kickoffId, CancellationToken token = default)
    {
        return ctx.WithAdvisoryLockAsync($"kickoff:{kickoffId}", async ct =>
        {
            var kickoff = await ctx.Kickoffs.FirstOrDefaultAsync(k => k.Id == kickoffId, ct);
            if (kickoff is null) return false;
            if (kickoff.StartedAt is not null) return true;

            var now = time.GetUtcNow();
            if (kickoff.StartsAt > now) return false; // Moved to a later date after it was picked up.

            kickoff.StartedAt = now;
            await ctx.SaveChangesAsync(ct);
            return true;
        }, token);
    }

    #endregion

    #region Members

    public Task<Kickoff> AddUsersAsync(Guid kickoffId, IReadOnlyCollection<Guid> userIds, CancellationToken token = default)
    {
        return ctx.WithAdvisoryLockAsync($"kickoff:{kickoffId}", async ct =>
        {
            var kickoff = await ctx.Kickoffs.AsNoTracking().FirstOrDefaultAsync(k => k.Id == kickoffId, ct)
                ?? throw new ServiceException(404, "Kickoff not found.");

            var ids = userIds.Distinct().ToList();
            var found = await ctx.Users
                .AsNoTracking()
                .Where(u => ids.Contains(u.Id))
                .Select(u => new { u.Id, u.Role, u.KickoffId })
                .ToListAsync(ct);

            var missing = ids.Except(found.Select(u => u.Id)).ToList();
            ServiceException.ThrowIf(missing.Count > 0, 404, $"Users not found: {Describe(missing)}.");

            // NOTE(W2): Staff never take part in kickoffs. Applicants get promoted by one, students
            // can still be cohorted but nothing changes for them.
            var staff = found.Where(u => u.Role is UserRole.Staff).Select(u => u.Id).ToList();
            ServiceException.ThrowIf(staff.Count > 0, 422, $"Staff can't take part in a kickoff: {Describe(staff)}.");

            var elsewhere = found.Where(u => u.KickoffId is not null && u.KickoffId != kickoffId).Select(u => u.Id).ToList();
            ServiceException.ThrowIf(elsewhere.Count > 0, 409,
                $"Already in another kickoff, remove them from it first: {Describe(elsewhere)}.");

            // Users that already are in this kickoff are skipped, which keeps a repeated call harmless.
            var toAdd = found.Where(u => u.KickoffId is null).Select(u => u.Id).ToList();
            if (toAdd.Count == 0)
                return kickoff;

            var members = await ctx.Users.CountAsync(u => u.KickoffId == kickoffId, ct);
            ServiceException.ThrowIf(members + toAdd.Count > kickoff.Capacity, 409,
                $"Not enough room in this kickoff: {Math.Max(kickoff.Capacity - members, 0)} seat(s) left, {toAdd.Count} requested.");

            var now = time.GetUtcNow();
            var affected = await ctx.Users
                .Where(u => toAdd.Contains(u.Id) && u.KickoffId == null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.KickoffId, (Guid?)kickoffId)
                    .SetProperty(u => u.UpdatedAt, now), ct);

            // Another kickoff claimed one of them in the meantime (it holds a different lock).
            // Throwing rolls the transaction back, so a batch is never half applied.
            ServiceException.ThrowIf(affected != toAdd.Count, 409,
                "A user was moved into another kickoff while adding them, please try again.");

            return kickoff;
        }, token);
    }

    public async Task RemoveUsersAsync(Guid kickoffId, IReadOnlyCollection<Guid> userIds, CancellationToken token = default)
    {
        ServiceException.ThrowIf(!await ctx.Kickoffs.AnyAsync(k => k.Id == kickoffId, token), 404, "Kickoff not found.");

        var ids = userIds.Distinct().ToList();
        var members = await ctx.Users
            .AsNoTracking()
            .Where(u => u.KickoffId == kickoffId && ids.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(token);

        var notMembers = ids.Except(members).ToList();
        ServiceException.ThrowIf(notMembers.Count > 0, 404, $"Not in this kickoff: {Describe(notMembers)}.");

        var now = time.GetUtcNow();
        await ctx.Users
            .Where(u => u.KickoffId == kickoffId && members.Contains(u.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.KickoffId, (Guid?)null)
                .SetProperty(u => u.UpdatedAt, now), token);
    }

    /// <summary>
    /// Names the offending ids in an error message, without letting a huge batch blow up the response.
    /// </summary>
    private static string Describe(IReadOnlyCollection<Guid> ids)
        => string.Join(", ", ids.Take(10)) + (ids.Count > 10 ? $" and {ids.Count - 10} more" : "");

    #endregion
}
