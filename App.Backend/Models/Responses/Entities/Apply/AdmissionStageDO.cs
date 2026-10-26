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
/// A check-in or an open day as it relates to one applicant.
/// </summary>
public class AdmissionStageDO
{
    public AdmissionStageDO() { }

    public AdmissionStageDO(CheckInRegistration registration)
    {
        RegistrationId = registration.Id;
        EventId = registration.EventId;
        EventName = registration.Event.Name;
        StartsAt = registration.Event.StartsAt;
        Attended = registration.AttendedAt is not null;
        AttendedAt = registration.AttendedAt;
    }

    public AdmissionStageDO(OpenDayRegistration registration)
    {
        RegistrationId = registration.Id;
        EventId = registration.EventId;
        EventName = registration.Event.Name;
        StartsAt = registration.Event.StartsAt;
        Attended = registration.AttendedAt is not null;
        AttendedAt = registration.AttendedAt;
    }

    [Required, Description("The sign-up.")]
    public Guid RegistrationId { get; set; }

    [Required, Description("The session they signed up for.")]
    public Guid EventId { get; set; }

    [Required, Description("The name of that session.")]
    public string EventName { get; set; } = null!;

    [Required, Description("When that session begins.")]
    public DateTimeOffset StartsAt { get; set; }

    [Required, Description("Whether a staff member verified they were there.")]
    public bool Attended { get; set; }

    [Description("When that was verified. Null if it wasn't.")]
    public DateTimeOffset? AttendedAt { get; set; }
}
