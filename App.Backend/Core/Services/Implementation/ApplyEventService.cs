// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Query;
using App.Backend.Core.Services.Interface;
using App.Backend.Core.Services.Options;
using App.Backend.Database;
using App.Backend.Database.Extensions;
using App.Backend.Domain.Entities.Apply;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

/// <summary>
/// Everything a check-in and an open day have in common: the capacity and the deadline, the sign-ups, and
/// the staff-verified record of who showed up. The two only differ in what they require before someone may
/// sign up, which is what <see cref="EnsureEligibleAsync"/> is for.
/// </summary>
/// <typeparam name="TEvent"></typeparam>
/// <typeparam name="TRegistration"></typeparam>
/// <param name="ctx"></param>
/// <param name="time"></param>
/// <param name="options"></param>
public abstract class ApplyEventService<TEvent, TRegistration>(
    DatabaseContext ctx,
    TimeProvider time,
    IOptions<AdmissionOptions> options
) : BaseService<TEvent>(ctx), IApplyEventService<TEvent, TRegistration>
    where TEvent : ApplyEvent
    where TRegistration : ApplyRegistration<TEvent>, new()
{
    protected DatabaseContext Db { get; } = ctx;

    protected AdmissionOptions Rules => options.Value;

    protected DbSet<TRegistration> Registrations => Db.Set<TRegistration>();

    /// <summary>
    /// What a session is called in messages, without an article. E.g: "check-in".
    /// </summary>
    protected abstract string Label { get; }

    /// <summary>
    /// Prefix of the lock names, so the kinds of session never share a lock. E.g: "checkin".
    /// </summary>
    protected abstract string LockScope { get; }

    /// <summary>
    /// Whatever else someone has to have done before they may sign up. Throws a <see cref="ServiceException"/> if not.
    /// Runs under the lock of the person.
    /// </summary>
    protected virtual Task EnsureEligibleAsync(string email, CancellationToken token) => Task.CompletedTask;

    /// <summary>
    /// Whatever stops someone's presence from being taken back, i.e. they already took the next step.
    /// Throws a <see cref="ServiceException"/> if so. Runs under the lock of the person.
    /// </summary>
    protected virtual Task EnsureCanRevokeAsync(string email, CancellationToken token) => Task.CompletedTask;

    #region Queries

    public async Task<IReadOnlyDictionary<Guid, ApplyCounts>> CountAsync(IReadOnlyCollection<Guid> eventIds, CancellationToken token = default)
    {
        if (eventIds.Count == 0)
            return new Dictionary<Guid, ApplyCounts>();

        var ids = eventIds.ToList();
        var counts = await Registrations
            .AsNoTracking()
            .Where(r => ids.Contains(r.EventId))
            .GroupBy(r => r.EventId)
            .Select(g => new {
                Id = g.Key,
                Registered = g.Count(),
                Attended = g.Count(r => r.AttendedAt != null)
            })
            .ToListAsync(token);

        return counts.ToDictionary(c => c.Id, c => new ApplyCounts(c.Registered, c.Attended));
    }

    public async Task<PaginatedList<TRegistration>> GetRegistrationsAsync(
        Guid eventId,
        string? email,
        bool? attended,
        ISorting sorting,
        IPagination pagination,
        CancellationToken token = default
    )
    {
        var query = Registrations.AsNoTracking().Where(r => r.EventId == eventId);

        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(r => r.Email.Contains(email.Trim(), StringComparison.InvariantCultureIgnoreCase));
        if (attended is true)
            query = query.Where(r => r.AttendedAt != null);
        else if (attended is false)
            query = query.Where(r => r.AttendedAt == null);

        return await query.Sort(sorting).PaginateAsync(pagination, token);
    }

    #endregion

    #region Session

    public override Task<TEvent> CreateAsync(TEvent entity, CancellationToken token = default)
    {
        ServiceException.ThrowIf(entity.DeadlineAt > entity.StartsAt, 422, "The sign-up deadline can't be after the start.");
        ServiceException.ThrowIf(entity.Capacity is < 1, 422, "The capacity must be at least 1, or empty for no limit.");
        return base.CreateAsync(entity, token);
    }

    public override Task UpdateAsync(TEvent entity, CancellationToken token = default)
    {
        return Db.WithAdvisoryLockAsync($"{LockScope}:{entity.Id}", async ct =>
        {
            ServiceException.ThrowIf(!await _dbSet.AnyAsync(e => e.Id == entity.Id, ct), 404, $"The {Label} doesn't exist.");
            ServiceException.ThrowIf(entity.DeadlineAt > entity.StartsAt, 422, "The sign-up deadline can't be after the start.");
            ServiceException.ThrowIf(entity.Capacity is < 1, 422, "The capacity must be at least 1, or empty for no limit.");

            // We hold the lock, so nobody can sign up between this count and the save.
            if (entity.Capacity is { } capacity)
            {
                var taken = await Registrations.CountAsync(r => r.EventId == entity.Id, ct);
                ServiceException.ThrowIf(capacity < taken, 409,
                    $"The capacity can't be lower than the current amount of sign-ups ({taken}).");
            }

            await base.UpdateAsync(entity, ct);
        }, token);
    }

    public override Task DeleteAsync(TEvent entity, CancellationToken token = default)
    {
        return Db.WithAdvisoryLockAsync($"{LockScope}:{entity.Id}", async ct =>
        {
            var attended = await Registrations.CountAsync(r => r.EventId == entity.Id && r.AttendedAt != null, ct);
            ServiceException.ThrowIf(attended > 0, 409,
                $"{attended} person(s) were verified at this {Label}, that record has to stay.");

            // What is left are sign-ups that never turned into attendance, those go with the session.
            await Registrations.Where(r => r.EventId == entity.Id).ExecuteDeleteAsync(ct);

            _dbSet.Remove(entity);
            await Db.SaveChangesAsync(ct);
        }, token);
    }

    #endregion

    #region Registrations

    public Task<TRegistration> RegisterAsync(Guid eventId, string email, string? firstName, string? lastName, CancellationToken token = default)
    {
        var normalized = email.Trim().ToLowerInvariant();;
        ServiceException.ThrowIf(normalized.Length == 0, 422, "An email address is required.");

        return Db.WithAdvisoryLockAsync($"{LockScope}:{eventId}", async ct =>
        {
            var session = await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId, ct)
                ?? throw new ServiceException(404, $"The {Label} doesn't exist.");

            // Repeating a call is harmless. This goes before the checks below on purpose: someone who is in
            // already and has since been verified, or whose deadline has passed, still gets their row back.
            var existing = await Registrations.FirstOrDefaultAsync(r => r.EventId == eventId && r.Email == normalized, ct);
            if (existing is not null)
                return existing;

            var now = time.GetUtcNow();
            ServiceException.ThrowIf(now > session.DeadlineAt, 409, $"Sign-up for this {Label} is closed.");

            return await Db.WithAdvisoryLockAsync($"apply-email:{normalized}", async ct2 =>
            {
                await EnsureEligibleAsync(normalized, ct2);

                var verified = await Registrations.AnyAsync(r => r.Email == normalized && r.AttendedAt != null, ct2);
                ServiceException.ThrowIf(verified, 409, $"{normalized} was already verified at another {Label}.");

                // One seat at a time, so nobody can hold a seat in every session.
                var waiting = await Registrations.AnyAsync(r => r.Email == normalized && r.AttendedAt == null && r.Event.StartsAt > now, ct2);
                ServiceException.ThrowIf(waiting, 409, $"{normalized} is already signed up for another {Label} that hasn't started yet, remove that first.");

                if (session.Capacity is { } capacity)
                {
                    var taken = await Registrations.CountAsync(r => r.EventId == eventId, ct2);
                    ServiceException.ThrowIf(taken >= capacity, 409, $"This {Label} is full ({capacity} seat(s)).");
                }

                var registration = new TRegistration
                {
                    EventId = eventId,
                    Email = normalized,
                    FirstName = Clean(firstName),
                    LastName = Clean(lastName),
                };

                await Registrations.AddAsync(registration, ct2);
                await Db.SaveChangesAsync(ct2);
                return registration;
            }, ct);
        }, token);
    }

    public Task UnregisterAsync(Guid eventId, Guid registrationId, CancellationToken token = default)
    {
        return Db.WithAdvisoryLockAsync($"{LockScope}:{eventId}", async ct =>
        {
            var registration = await Registrations.FirstOrDefaultAsync(r => r.Id == registrationId && r.EventId == eventId, ct)
                ?? throw new ServiceException(404, "This isn't a sign-up for this session.");

            ServiceException.ThrowIf(registration.AttendedAt is not null, 409,
                "This person was verified as present, that record has to stay.");

            Registrations.Remove(registration);
            await Db.SaveChangesAsync(ct);
        }, token);
    }

    public Task<TRegistration> SetAttendanceAsync(Guid eventId, Guid registrationId, bool attended, Guid verifiedBy, CancellationToken token = default)
    {
        return Db.WithAdvisoryLockAsync($"{LockScope}:{eventId}", async ct =>
        {
            var session = await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId, ct)
                ?? throw new ServiceException(404, $"The {Label} doesn't exist.");

            var registration = await Registrations.FirstOrDefaultAsync(r => r.Id == registrationId && r.EventId == eventId, ct)
                ?? throw new ServiceException(404, "This isn't a sign-up for this session.");

            if (attended)
            {
                // Whoever verified it first stays on record, a second call doesn't take that over.
                if (registration.AttendedAt is not null)
                    return registration;

                var now = time.GetUtcNow();
                var opens = session.StartsAt - Rules.AttendanceOpensBeforeStart;
                var closes = session.StartsAt + Rules.AttendanceClosesAfterStart;
                ServiceException.ThrowIf(now < opens, 409,
                    $"Too early, presence can be recorded from {opens.UtcDateTime:yyyy-MM-dd HH:mm} UTC.");
                ServiceException.ThrowIf(now > closes, 409,
                    $"Too late, presence could be recorded until {closes.UtcDateTime:yyyy-MM-dd HH:mm} UTC.");

                registration.AttendedAt = now;
                registration.VerifiedBy = verifiedBy;
                await Db.SaveChangesAsync(ct);
                return registration;
            }

            if (registration.AttendedAt is null)
                return registration;

            // Taking presence back is a correction. The person's lock makes sure nobody is signing up for
            // the next step right now, and the next step is what decides whether this is still possible.
            return await Db.WithAdvisoryLockAsync($"apply-email:{registration.Email}", async ct2 =>
            {
                await EnsureCanRevokeAsync(registration.Email, ct2);

                registration.AttendedAt = null;
                registration.VerifiedBy = null;
                await Db.SaveChangesAsync(ct2);
                return registration;
            }, ct);
        }, token);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    #endregion
}
