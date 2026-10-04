// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Query;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

/// <summary>
/// Kickoffs and their members.
///
/// A kickoff is a launch date for a cohort. Users are put into one (by staff, or by the applicant
/// through the kickoff selection app) and once it starts, its applicants are promoted to students.
/// </summary>
/// <remarks>
/// Everything here is all-or-nothing: if any part of a request is invalid a <see cref="ServiceException"/>
/// is thrown and nothing is changed.
///
/// <see cref="IDomainService{T}.UpdateAsync"/> refuses (409) to drop the capacity below the member count and to
/// move the date of a kickoff that already started. <see cref="IDomainService{T}.DeleteAsync"/> refuses (409)
/// while the kickoff still has members.
/// </remarks>
public interface IKickoffService : IDomainService<Kickoff>
{
    /// <summary>
    /// Counts the members of several kickoffs in one query.
    /// </summary>
    /// <param name="kickoffIds">The kickoffs.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Member count by kickoff id. Kickoffs without members are absent.</returns>
    Task<IReadOnlyDictionary<Guid, int>> CountMembersAsync(IReadOnlyCollection<Guid> kickoffIds, CancellationToken token = default);

    /// <summary>
    /// Finds the kickoff a user belongs to.
    /// </summary>
    /// <param name="userId">The user.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The kickoff, or null if the user has none or doesn't exist.</returns>
    Task<Kickoff?> FindByUserAsync(Guid userId, CancellationToken token = default);

    /// <summary>
    /// The members of a kickoff.
    /// </summary>
    /// <param name="kickoffId">The kickoff.</param>
    /// <param name="role">Only users with this role, e.g. Applicant for the ones still waiting to be promoted.</param>
    /// <param name="sorting">The sorting.</param>
    /// <param name="pagination">The pagination.</param>
    /// <param name="token">Cancellation token.</param>
    Task<PaginatedList<User>> GetUsersAsync(
        Guid kickoffId,
        UserRole? role,
        ISorting sorting,
        IPagination pagination,
        CancellationToken token = default
    );

    /// <summary>
    /// The ids of the members of a kickoff with the given role.
    /// </summary>
    /// <param name="kickoffId">The kickoff.</param>
    /// <param name="role">The role to look for.</param>
    /// <param name="token">Cancellation token.</param>
    Task<IReadOnlyList<Guid>> GetUserIdsAsync(Guid kickoffId, UserRole role, CancellationToken token = default);

    /// <summary>
    /// Adds users to a kickoff. Users that are already in this kickoff are skipped, so repeating a call is safe.
    /// </summary>
    /// <param name="kickoffId">The kickoff.</param>
    /// <param name="userIds">The users to add.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>
    /// The kickoff as it was when the users were added. If it already started it will never run again,
    /// so the caller has to get the added applicants promoted themselves.
    /// </returns>
    /// <exception cref="ServiceException">
    /// 404 if the kickoff or any user is missing. 422 if any user is staff. 409 if any user is in
    /// another kickoff or the users don't fit. The message names the offending users.
    /// </exception>
    Task<Kickoff> AddUsersAsync(Guid kickoffId, IReadOnlyCollection<Guid> userIds, CancellationToken token = default);

    /// <summary>
    /// Removes users from a kickoff. Their role is left as is, a promoted student stays a student.
    /// </summary>
    /// <param name="kickoffId">The kickoff.</param>
    /// <param name="userIds">The users to remove.</param>
    /// <param name="token">Cancellation token.</param>
    /// <exception cref="ServiceException">404 if the kickoff is missing or any of the users isn't in it.</exception>
    Task RemoveUsersAsync(Guid kickoffId, IReadOnlyCollection<Guid> userIds, CancellationToken token = default);

    /// <summary>
    /// Marks a kickoff as started once it is due. Safe to repeat.
    /// </summary>
    /// <param name="kickoffId">The kickoff.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>True if the kickoff is started (now or before). False if it doesn't exist or isn't due (anymore).</returns>
    Task<bool> EnsureStartedAsync(Guid kickoffId, CancellationToken token = default);
}
