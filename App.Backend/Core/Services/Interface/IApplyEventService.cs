// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Query;
using App.Backend.Domain.Entities.Apply;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

/// <summary>
/// How many people signed up for a session, and how many of them were verified as present.
/// </summary>
public readonly record struct ApplyCounts(int Registered, int Attended);

public interface IApplyEventService<TEvent, TRegistration> : IDomainService<TEvent>
    where TEvent : ApplyEvent
    where TRegistration : ApplyRegistration<TEvent>
{
    /// <summary>
    /// Counts the sign-ups of several sessions in one query.
    /// </summary>
    /// <param name="eventIds">The sessions.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Counts by session id. Sessions without sign-ups are absent.</returns>
    Task<IReadOnlyDictionary<Guid, ApplyCounts>> CountAsync(IReadOnlyCollection<Guid> eventIds, CancellationToken token = default);

    /// <summary>
    /// The sign-ups of a session.
    /// </summary>
    /// <param name="eventId">The session.</param>
    /// <param name="email">Only sign-ups whose email contains this.</param>
    /// <param name="attended">Only the ones that were (true) or weren't (false) verified as present.</param>
    /// <param name="sorting">The sorting.</param>
    /// <param name="pagination">The pagination.</param>
    /// <param name="token">Cancellation token.</param>
    Task<PaginatedList<TRegistration>> GetRegistrationsAsync(
        Guid eventId,
        string? email,
        bool? attended,
        ISorting sorting,
        IPagination pagination,
        CancellationToken token = default
    );

    /// <summary>
    /// Signs someone up for a session. Repeating a call is safe, the existing sign-up is returned.
    /// </summary>
    /// <param name="eventId">The session.</param>
    /// <param name="email">Their email address, it doesn't need to be normalized.</param>
    /// <param name="firstName">Their first name, if known.</param>
    /// <param name="lastName">Their last name, if known.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The sign-up.</returns>
    /// <exception cref="ServiceException">
    /// 404 if the session is missing. 409 if the deadline passed, the session is full, the person was already
    /// verified at a session of this kind, or they are signed up for another one that hasn't started. Open days
    /// also answer 409 for someone who wasn't verified at a check-in (see <c>AdmissionOptions</c>).
    /// </exception>
    Task<TRegistration> RegisterAsync(Guid eventId, string email, string? firstName, string? lastName, CancellationToken token = default);

    /// <summary>
    /// Removes a sign-up that never turned into attendance.
    /// </summary>
    /// <param name="eventId">The session.</param>
    /// <param name="registrationId">The sign-up.</param>
    /// <param name="token">Cancellation token.</param>
    /// <exception cref="ServiceException">404 if it isn't a sign-up for this session. 409 if the person was verified as present.</exception>
    Task UnregisterAsync(Guid eventId, Guid registrationId, CancellationToken token = default);

    /// <summary>
    /// Records, or takes back, that a staff member saw this person at the session. Repeating a call is safe
    /// and never overwrites who verified it first.
    /// </summary>
    /// <param name="eventId">The session.</param>
    /// <param name="registrationId">The sign-up.</param>
    /// <param name="attended">True to record presence, false to take it back.</param>
    /// <param name="verifiedBy">The staff member doing the verifying.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The sign-up as it is now.</returns>
    /// <exception cref="ServiceException">
    /// 404 if it isn't a sign-up for this session. 409 if it is too early or too late to record presence, or if
    /// presence can't be taken back because the person already took the next step.
    /// </exception>
    Task<TRegistration> SetAttendanceAsync(Guid eventId, Guid registrationId, bool attended, Guid verifiedBy, CancellationToken token = default);
}
