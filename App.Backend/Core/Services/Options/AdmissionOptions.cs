// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Core.Services.Options;

/// <summary>
/// Configuration options for the admissions pipeline: CheckIn -> OpenDay -> Trial -> Kickoff.
/// </summary>
public class AdmissionOptions
{
    public const string SectionName = "Admissions";

    /// <summary>
    /// Whether signing up for an open day requires having been verified at a check-in.
    /// </summary>
    public bool RequireCheckInForOpenDay { get; init; } = true;

    /// <summary>
    /// Whether an applicant can only join a trial after having been verified at an open day.
    /// </summary>
    public bool RequireOpenDayForTrial { get; init; } = false;

    /// <summary>
    /// Whether an applicant can only join a kickoff after having been selected in a trial.
    /// </summary>
    public bool RequireTrialForKickoff { get; init; } = false;

    /// <summary>
    /// How long before a session starts staff can begin verifying who is there.
    /// </summary>
    public TimeSpan AttendanceOpensBeforeStart { get; init; } = TimeSpan.FromHours(2);

    /// <summary>
    /// How long after a session starts when a session ends.
    /// </summary>
    public TimeSpan AttendanceClosesAfterStart { get; init; } = TimeSpan.FromDays(1);
}
