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

public class TrialService(DatabaseContext ctx, TimeProvider time) : BaseService<Trial>(ctx), ITrialService
{
    private readonly DatabaseContext ctx = ctx;

    #region Queries

    public async Task<IReadOnlyDictionary<Guid, int>> CountParticipantsAsync(IReadOnlyCollection<Guid> trialIds, CancellationToken token = default)
    {
        if (trialIds.Count == 0)
            return new Dictionary<Guid, int>();

        var ids = trialIds.ToList();
        var counts = await ctx.UserTrials
            .AsNoTracking()
            .Where(ut => ids.Contains(ut.TrialId))
            .GroupBy(ut => ut.TrialId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToListAsync(token);

        return counts.ToDictionary(c => c.Id, c => c.Count);
    }

    public async Task<PaginatedList<UserTrial>> GetParticipantsAsync(
        Guid trialId,
        UserTrialState? state,
        TrialOutcome? outcome,
        ISorting sorting,
        IPagination pagination,
        CancellationToken token = default
    )
    {
        var query = ctx.UserTrials
            .AsNoTracking()
            .Include(ut => ut.User)
            .Where(ut => ut.TrialId == trialId);

        if (state.HasValue)
            query = query.Where(ut => ut.State == state.Value);
        if (outcome.HasValue)
            query = query.Where(ut => ut.Outcome == outcome.Value);

        return await query.Sort(sorting).PaginateAsync(pagination, token);
    }

    public async Task<IReadOnlyList<Guid>> GetUserIdsAsync(Guid trialId, UserTrialState state, CancellationToken token = default)
    {
        return await ctx.UserTrials
            .AsNoTracking()
            .Where(ut => ut.TrialId == trialId && ut.State == state)
            .Select(ut => ut.UserId)
            .ToListAsync(token);
    }

    public async Task<UserTrial?> FindParticipantAsync(Guid trialId, Guid userId, CancellationToken token = default)
    {
        return await ctx.UserTrials
            .AsNoTracking()
            .Include(ut => ut.User)
            .FirstOrDefaultAsync(ut => ut.TrialId == trialId && ut.UserId == userId, token);
    }

    public async Task<IReadOnlyList<UserTrial>> GetByUserAsync(Guid userId, CancellationToken token = default)
    {
        return await ctx.UserTrials
            .AsNoTracking()
            .Include(ut => ut.User)
            .Where(ut => ut.UserId == userId)
            .OrderByDescending(ut => ut.CreatedAt)
            .ToListAsync(token);
    }

    #endregion

    #region Trial

    public override async Task<Trial> CreateAsync(Trial entity, CancellationToken token = default)
    {
        ServiceException.ThrowIf(entity.EndsAt <= entity.StartsAt, 422, "A trial must end after it starts.");
        ServiceException.ThrowIf(entity.EndsAt <= time.GetUtcNow(), 422, "A trial can't end in the past.");
        ServiceException.ThrowIf(!await ctx.Cursi.AnyAsync(c => c.Id == entity.CursusId, token), 422, "Cursus not found.");

        // NOTE: Dates in the past are fine for the start, it simply begins on the next run of the trial job.
        return await base.CreateAsync(entity, token);
    }

    public override Task UpdateAsync(Trial entity, CancellationToken token = default)
    {
        return ctx.WithAdvisoryLockAsync($"trial:{entity.Id}", async ct =>
        {
            // Compare against what is persisted, not what the caller handed us. We hold the lock,
            // so neither the participants nor the start and end can change underneath these checks.
            var persisted = await ctx.Trials
                .AsNoTracking()
                .Where(t => t.Id == entity.Id)
                .Select(t => new { t.CursusId, t.StartsAt, t.EndsAt, t.StartedAt, t.EndedAt })
                .FirstOrDefaultAsync(ct)
                ?? throw new ServiceException(404, "Trial not found.");

            ServiceException.ThrowIf(entity.EndsAt <= entity.StartsAt, 422, "A trial must end after it starts.");

            var participants = await ctx.UserTrials.CountAsync(ut => ut.TrialId == entity.Id, ct);
            ServiceException.ThrowIf(entity.Capacity < participants, 409,
                $"The capacity can't be lower than the current amount of participants ({participants}).");

            ServiceException.ThrowIf(persisted.StartedAt is not null && entity.StartsAt != persisted.StartsAt, 409,
                "This trial has already started, its start date can no longer change.");

            ServiceException.ThrowIf(persisted.StartedAt is not null && entity.CursusId != persisted.CursusId, 409,
                "This trial has already started, its cursus can no longer change.");

            // NOTE(W2): Moving the end of a running trial is how you end it early, so that is fine, up until it did end.
            ServiceException.ThrowIf(persisted.EndedAt is not null && entity.EndsAt != persisted.EndsAt, 409,
                "This trial has already ended, its end date can no longer change.");

            if (entity.CursusId != persisted.CursusId)
                ServiceException.ThrowIf(!await ctx.Cursi.AnyAsync(c => c.Id == entity.CursusId, ct), 422, "Cursus not found.");

            entity.StartedAt = persisted.StartedAt;
            entity.EndedAt = persisted.EndedAt;
            await base.UpdateAsync(entity, ct);
        }, token);
    }

    public override async Task DeleteAsync(Trial entity, CancellationToken token = default)
    {
        await ctx.WithAdvisoryLockAsync($"trial:{entity.Id}", async ct =>
        {
            var participants = await ctx.UserTrials.CountAsync(ut => ut.TrialId == entity.Id, ct);
            ServiceException.ThrowIf(participants > 0, 409,
                $"This trial still has {participants} participant(s), remove them first.");

            ctx.Trials.Remove(entity);
            await ctx.SaveChangesAsync(ct);
        }, token);
    }

    public Task<bool> StartTrialAsync(Guid trialId, CancellationToken token = default)
    {
        return ctx.WithAdvisoryLockAsync($"trial:{trialId}", async ct =>
        {
            var trial = await ctx.Trials.FirstOrDefaultAsync(t => t.Id == trialId, ct);
            if (trial is null) return false;
            if (trial.StartedAt is not null) return true;

            var now = time.GetUtcNow();
            if (trial.StartsAt > now) return false; // Moved to a later date after it was picked up.

            trial.StartedAt = now;
            await ctx.SaveChangesAsync(ct);
            return true;
        }, token);
    }

    public Task<bool> StopTrialAsync(Guid trialId, CancellationToken token = default)
    {
        return ctx.WithAdvisoryLockAsync($"trial:{trialId}", async ct =>
        {
            var trial = await ctx.Trials.FirstOrDefaultAsync(t => t.Id == trialId, ct);
            if (trial is null) return false;
            if (trial.EndedAt is not null) return true;
            if (trial.StartedAt is null) return false; // It never ran, so the start has to go first.

            var now = time.GetUtcNow();
            if (trial.EndsAt > now) return false; // Moved to a later date after it was picked up.

            // Whoever was taking part made it to the end.
            await ctx.UserTrials
                .Where(ut => ut.TrialId == trialId && ut.State == UserTrialState.Active)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(ut => ut.State, UserTrialState.Completed)
                    .SetProperty(ut => ut.EndedAt, (DateTimeOffset?)now)
                    .SetProperty(ut => ut.UpdatedAt, now), ct);

            // Whoever never became active never showed up. Leaving them registered would also keep them
            // from joining any other trial, since that is an open state.
            await ctx.UserTrials
                .Where(ut => ut.TrialId == trialId && ut.State == UserTrialState.Registered)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(ut => ut.State, UserTrialState.Quit)
                    .SetProperty(ut => ut.EndedAt, (DateTimeOffset?)now)
                    .SetProperty(ut => ut.UpdatedAt, now), ct);

            trial.EndedAt = now;
            await ctx.SaveChangesAsync(ct);
            return true;
        }, token);
    }

    #endregion

    #region Participants

    public Task<UserTrial> JoinAsync(Guid trialId, Guid userId, CancellationToken token = default)
    {
        return ctx.WithAdvisoryLockAsync($"trial:{trialId}", async ct =>
        {
            var trial = await ctx.Trials.FirstOrDefaultAsync(t => t.Id == trialId, ct)
                ?? throw new ServiceException(404, "Trial not found.");

            var user = await ctx.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                ?? throw new ServiceException(404, "User not found.");

            // Repeating a call is harmless. This goes before the checks below on purpose: a user that
            // is already in, and has since become a student or whose trial ended, still gets their row back.
            var existing = await ctx.UserTrials.FirstOrDefaultAsync(ut => ut.TrialId == trialId && ut.UserId == userId, ct);
            if (existing is not null)
            {
                ServiceException.ThrowIf(existing.State is UserTrialState.Quit, 409, "This user already quit this trial.");
                return existing;
            }

            // NOTE(W2): Trials are the applicant stage. Students are already in and staff never take part.
            ServiceException.ThrowIf(user.Role is not UserRole.Applicant, 422, "Only applicants can take part in a trial.");
            ServiceException.ThrowIf(trial.EndedAt is not null || trial.EndsAt <= time.GetUtcNow(), 409, "This trial is already over.");

            // The same user may be signing up for another trial at this very moment, which holds a different
            // trial lock. Always take the trial lock first and the user lock second, or this could deadlock.
            return await ctx.WithAdvisoryLockAsync($"trial-user:{userId}", async ct2 =>
            {
                var busy = await ctx.UserTrials.AnyAsync(ut =>
                    ut.UserId == userId &&
                    (ut.State == UserTrialState.Registered || ut.State == UserTrialState.Active), ct2);
                ServiceException.ThrowIf(busy, 409, "This user is already part of another trial that hasn't finished.");

                var taken = await ctx.UserTrials.CountAsync(ut => ut.TrialId == trialId, ct2);
                ServiceException.ThrowIf(taken >= trial.Capacity, 409, $"This trial is full ({trial.Capacity} seat(s)).");

                var participation = new UserTrial
                {
                    Trial = trial,
                    User = user,
                    State = UserTrialState.Registered,
                    Outcome = TrialOutcome.Pending,
                };

                await ctx.UserTrials.AddAsync(participation, ct2);
                await ctx.SaveChangesAsync(ct2);
                return participation;
            }, ct);
        }, token);
    }

    public Task<UserTrial?> LeaveAsync(Guid trialId, Guid userId, CancellationToken token = default)
    {
        return ctx.WithAdvisoryLockAsync<UserTrial?>($"trial:{trialId}", async ct =>
        {
            ServiceException.ThrowIf(!await ctx.Trials.AnyAsync(t => t.Id == trialId, ct), 404, "Trial not found.");

            var participation = await ctx.UserTrials.FirstOrDefaultAsync(ut => ut.TrialId == trialId && ut.UserId == userId, ct)
                ?? throw new ServiceException(404, "This user isn't in this trial.");

            switch (participation.State)
            {
                case UserTrialState.Registered:
                    // Never started, so there is nothing to keep a record of.
                    ctx.UserTrials.Remove(participation);
                    await ctx.SaveChangesAsync(ct);
                    return null;

                case UserTrialState.Active:
                    participation.State = UserTrialState.Quit;
                    participation.EndedAt = time.GetUtcNow();
                    await ctx.SaveChangesAsync(ct);
                    return participation;

                case UserTrialState.Quit:
                    return participation; // Repeating a call is harmless.

                default:
                    throw new ServiceException(409, "This trial is already over for this user.");
            }
        }, token);
    }

    public Task SetOutcomeAsync(Guid trialId, IReadOnlyCollection<Guid> userIds, TrialOutcome outcome, CancellationToken token = default)
    {
        return ctx.WithAdvisoryLockAsync($"trial:{trialId}", async ct =>
        {
            ServiceException.ThrowIf(!await ctx.Trials.AnyAsync(t => t.Id == trialId, ct), 404, "Trial not found.");

            var ids = userIds.Distinct().ToList();
            var found = await ctx.UserTrials
                .AsNoTracking()
                .Where(ut => ut.TrialId == trialId && ids.Contains(ut.UserId))
                .Select(ut => new { ut.UserId, ut.State })
                .ToListAsync(ct);

            var missing = ids.Except(found.Select(f => f.UserId)).ToList();
            ServiceException.ThrowIf(missing.Count > 0, 404, $"Not in this trial: {Describe(missing)}.");

            // A verdict on someone still taking part would be a verdict on unfinished work.
            var notOver = found
                .Where(f => f.State is UserTrialState.Registered or UserTrialState.Active)
                .Select(f => f.UserId)
                .ToList();
            ServiceException.ThrowIf(notOver.Count > 0, 409, $"The trial isn't over for them yet: {Describe(notOver)}.");

            var now = time.GetUtcNow();
            var affected = await ctx.UserTrials
                .Where(ut => ut.TrialId == trialId
                    && ids.Contains(ut.UserId)
                    && (ut.State == UserTrialState.Completed || ut.State == UserTrialState.Quit))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(ut => ut.Outcome, outcome)
                    .SetProperty(ut => ut.UpdatedAt, now), ct);

            // Throwing rolls the transaction back, so a batch is never half applied.
            ServiceException.ThrowIf(affected != ids.Count, 409,
                "A participation changed while deciding, please try again.");
        }, token);
    }

    private static string Describe(IReadOnlyCollection<Guid> ids)
        => string.Join(", ", ids.Take(10)) + (ids.Count > 10 ? $" and {ids.Count - 10} more" : "");

    #endregion
}
