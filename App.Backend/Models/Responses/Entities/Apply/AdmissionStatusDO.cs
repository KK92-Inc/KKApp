// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Apply;

/// <summary>
/// Response object describing where an applicant stands on the way from check-in to kickoff.
/// </summary>
public class AdmissionStatusDO
{
    public AdmissionStatusDO() { }

    public AdmissionStatusDO(
        string email,
        AdmissionStep next,
        AdmissionStageDO? checkIn,
        AdmissionStageDO? openDay,
        Guid? userId,
        UserTrialDO? trial)
    {
        Email = email;
        Next = next;
        CheckIn = checkIn;
        OpenDay = openDay;
        UserId = userId;
        Trial = trial;
    }

    [Required, Description("The applicant's email address, lower-cased.")]
    public string Email { get; set; } = null!;

    [Required, Description("The step they still have to complete. This is the order of the pipeline, whatever is or isn't enforced.")]
    public AdmissionStep Next { get; set; }

    [Description("Their check-in: the one they were verified at, otherwise the latest they signed up for. Null if they never did.")]
    public AdmissionStageDO? CheckIn { get; set; }

    [Description("Their open day: the one they were verified at, otherwise the latest they signed up for. Null if they never did.")]
    public AdmissionStageDO? OpenDay { get; set; }

    [Description("The id of the account with this email. Null while there is none.")]
    public Guid? UserId { get; set; }

    [Description("Their trial: the one they were selected in, otherwise the latest. Null if they never took part in one.")]
    public UserTrialDO? Trial { get; set; }
}
