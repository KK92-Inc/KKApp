// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Apply;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Apply;

/// <summary>
/// Response object describing someone's sign-up for a check-in or an open day.
/// </summary>
public class ApplyRegistrationDO : BaseEntityDO<ApplyRegistration>
{
    public ApplyRegistrationDO() { }

    public ApplyRegistrationDO(ApplyRegistration registration) : base(registration)
    {
        EventId = registration.EventId;
        Email = registration.Email;
        FirstName = registration.FirstName;
        LastName = registration.LastName;
        AttendedAt = registration.AttendedAt;
        VerifiedBy = registration.VerifiedBy;
    }

    [Required, Description("The session this is a sign-up for.")]
    public Guid EventId { get; set; }

    [Required, Description("The applicant's email address, lower-cased.")]
    public string Email { get; set; } = null!;

    [Description("The applicant's first name, if known.")]
    public string? FirstName { get; set; }

    [Description("The applicant's last name, if known.")]
    public string? LastName { get; set; }

    [Description("When a staff member verified this person was there. Null if they weren't (yet).")]
    public DateTimeOffset? AttendedAt { get; set; }

    [Description("The id of the staff member who verified it. Null if nobody did.")]
    public Guid? VerifiedBy { get; set; }
}
