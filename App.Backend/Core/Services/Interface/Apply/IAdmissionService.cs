// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

// ============================================================================

using App.Backend.Domain.Values;

namespace App.Backend.Core.Services.Interface.Apply;

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
