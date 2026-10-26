// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Apply;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Domain.Values;

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
