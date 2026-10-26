// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Apply;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Core.Services.Interface;

/// <summary>
/// Where one applicant stands on the way from check-in to kickoff.
/// </summary>
/// <param name="Email">The normalized email this is about.</param>
/// <param name="Next">The step they still have to complete.</param>
/// <param name="CheckIn">Their verified check-in, or else the latest one they signed up for. Null if they never did.</param>
/// <param name="OpenDay">Their verified open day, or else the latest one they signed up for. Null if they never did.</param>
/// <param name="User">The account that has this email, once it exists. Null before that.</param>
/// <param name="Trial">The trial that decides their fate: the one they were selected in, otherwise the latest.</param>
public record AdmissionProgress(
    string Email,
    AdmissionStep Next,
    CheckInRegistration? CheckIn,
    OpenDayRegistration? OpenDay,
    User? User,
    UserTrial? Trial
);

/// <summary>
/// Puts the four steps of admissions together. Check-ins and open days are keyed by email,
/// trials and kickoffs by user, this is where the two meet.
/// </summary>
public interface IAdmissionService
{
    /// <summary>
    /// Works out where an applicant is in the pipeline. Never fails for an unknown email, that is
    /// simply someone who hasn't started.
    /// </summary>
    /// <param name="email">The applicant's email, it doesn't need to be normalized.</param>
    /// <param name="token">Cancellation token.</param>
    Task<AdmissionProgress> GetProgressAsync(string email, CancellationToken token = default);
}
