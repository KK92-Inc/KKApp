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
/// Trial periods and the users taking part in them.
///
/// A trial is a span of time in which applicants try to complete a cursus. Once it starts its
/// participants are activated, once it ends they are wrapped up, and afterwards staff decide
/// who was selected to become a student.
/// </summary>
/// <remarks>
/// Everything here is all-or-nothing: if any part of a request is invalid a <see cref="ServiceException"/>
/// is thrown and nothing is changed.
///
/// <see cref="IDomainService{T}.CreateAsync"/> refuses (422) a trial that ends before it starts or in the past.
/// <see cref="IDomainService{T}.UpdateAsync"/> refuses (409) to drop the capacity below the participant count,
/// to move the start or swap the cursus of a trial that already started, and to move the end of one that
/// already ended. <see cref="IDomainService{T}.DeleteAsync"/> refuses (409) while the trial still has
/// participants, so the history of who took part is never lost by accident.
/// </remarks>
public interface ITrialService : IDomainService<Trial>
{
    /// <summary>
    /// Counts the participants of several trials in one query. Everyone counts, including
    /// those that quit, since a seat in a cohort isn't handed back once the trial runs.
    /// </summary>
    /// <param name="trialIds">The trials.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Participant count by trial id. Trials without participants are absent.</returns>
    Task<IReadOnlyDictionary<Guid, int>> CountParticipantsAsync(IReadOnlyCollection<Guid> trialIds, CancellationToken token = default);

    /// <summary>
    /// The participants of a trial, with their user.
    /// </summary>
    /// <param name="trialId">The trial.</param>
    /// <param name="state">Only participants in this state.</param>
    /// <param name="outcome">Only participants with this verdict, e.g. Pending for the ones still waiting for one.</param>
    /// <param name="sorting">The sorting.</param>
    /// <param name="pagination">The pagination.</param>
    /// <param name="token">Cancellation token.</param>
    Task<PaginatedList<UserTrial>> GetParticipantsAsync(
        Guid trialId,
        UserTrialState? state,
        TrialOutcome? outcome,
        ISorting sorting,
        IPagination pagination,
        CancellationToken token = default
    );

    /// <summary>
    /// The ids of the participants of a trial that are in the given state.
    /// </summary>
    /// <param name="trialId">The trial.</param>
    /// <param name="state">The state to look for.</param>
    /// <param name="token">Cancellation token.</param>
    Task<IReadOnlyList<Guid>> GetUserIdsAsync(Guid trialId, UserTrialState state, CancellationToken token = default);

    /// <summary>
    /// Finds the participation of a user in a trial, with the user.
    /// </summary>
    /// <param name="trialId">The trial.</param>
    /// <param name="userId">The user.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The participation, or null if the user isn't in the trial.</returns>
    Task<UserTrial?> FindParticipantAsync(Guid trialId, Guid userId, CancellationToken token = default);

    /// <summary>
    /// Every trial a user has taken part in, newest first, with the user.
    /// </summary>
    /// <param name="userId">The user.</param>
    /// <param name="token">Cancellation token.</param>
    Task<IReadOnlyList<UserTrial>> GetByUserAsync(Guid userId, CancellationToken token = default);

    /// <summary>
    /// Signs an applicant up for a trial. Repeating a call is safe, the existing participation is returned.
    /// </summary>
    /// <param name="trialId">The trial.</param>
    /// <param name="userId">The user.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>
    /// The participation, with its trial and user. If the trial is already running it will never
    /// start again, so when the state is Registered the caller has to get the user activated themselves.
    /// </returns>
    /// <exception cref="ServiceException">
    /// 404 if the trial or user is missing. 422 if the user is not an applicant. 409 if the trial is over or full,
    /// the user already quit it, or the user is part of another trial that hasn't finished.
    /// </exception>
    Task<UserTrial> JoinAsync(Guid trialId, Guid userId, CancellationToken token = default);

    /// <summary>
    /// Takes a user out of a trial. Someone who hasn't started yet is simply removed. Someone who is
    /// already taking part is marked as having quit, and the participation is kept as a record.
    /// </summary>
    /// <param name="trialId">The trial.</param>
    /// <param name="userId">The user.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>
    /// Null if the user hadn't started and was removed. Otherwise the participation that is now
    /// marked as quit, the caller has to get that user's account deactivated.
    /// </returns>
    /// <exception cref="ServiceException">404 if the trial is missing or the user isn't in it. 409 if the trial is over for them already.</exception>
    Task<UserTrial?> LeaveAsync(Guid trialId, Guid userId, CancellationToken token = default);

    /// <summary>
    /// Records the verdict of staff on the participants of a trial.
    /// </summary>
    /// <param name="trialId">The trial.</param>
    /// <param name="userIds">The users to decide on.</param>
    /// <param name="outcome">The verdict. Pending clears an earlier one.</param>
    /// <param name="token">Cancellation token.</param>
    /// <exception cref="ServiceException">
    /// 404 if the trial is missing or any of the users isn't in it. 409 if the trial isn't over for
    /// any of them yet. The message names the offending users.
    /// </exception>
    Task SetOutcomeAsync(Guid trialId, IReadOnlyCollection<Guid> userIds, TrialOutcome outcome, CancellationToken token = default);

    /// <summary>
    /// Marks a trial as started once it is due. Safe to repeat.
    /// </summary>
    /// <param name="trialId">The trial.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>True if the trial is started (now or before). False if it doesn't exist or isn't due (anymore).</returns>
    Task<bool> StartTrialAsync(Guid trialId, CancellationToken token = default);

    /// <summary>
    /// Marks a trial as ended once it is due, and wraps up its participants: the ones that were active
    /// are completed, the ones that never became active are marked as having quit. Safe to repeat.
    /// </summary>
    /// <param name="trialId">The trial.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>True if the trial is ended (now or before). False if it doesn't exist, hasn't started or isn't due (anymore).</returns>
    Task<bool> StopTrialAsync(Guid trialId, CancellationToken token = default);
}
