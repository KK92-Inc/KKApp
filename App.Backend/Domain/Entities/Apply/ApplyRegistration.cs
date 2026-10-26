// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities.Apply;

/// <summary>
/// Someone's sign-up for a session, and the record of whether they actually showed up.
///
/// Applicants don't have an account here yet, they are identified by their email address only.
/// The account is created later on and is matched back to these rows by that same email.
///
/// Signing up and showing up are deliberately two separate facts: <see cref="AttendedAt"/>
/// is only ever set by a staff member who saw the person, never by the applicant or by the
/// system that took the sign-up.
/// </summary>
public abstract class ApplyRegistration : BaseEntity
{
    /// <summary>
    /// The session this is a sign-up for. Which table it points at depends on the kind of registration.
    /// </summary>
    [Column("event_id")]
    public Guid EventId { get; set; }

    /// <summary>
    /// Always stored through <see cref="NormalizeEmail"/>, so it can be compared with plain equality.
    /// </summary>
    [Column("email"), StringLength(320)]
    public string Email { get; set; } = null!;

    [Column("first_name"), StringLength(255)]
    public string? FirstName { get; set; }

    [Column("last_name"), StringLength(255)]
    public string? LastName { get; set; }

    /// <summary>
    /// When a staff member verified this person was there. Null if they weren't (yet).
    /// Set by the server clock, a caller can't backdate it.
    /// </summary>
    [Column("attended_at")]
    public DateTimeOffset? AttendedAt { get; set; }

    /// <summary>
    /// Who verified the attendance, so there is always someone to ask. This is an id and not a
    /// foreign key on purpose: anonymizing or removing a staff member must not break, or erase, the record.
    /// </summary>
    [Column("verified_by")]
    public Guid? VerifiedBy { get; set; }
}

/// <summary>
/// A registration for one specific kind of session.
/// </summary>
/// <typeparam name="TEvent">The session this registration belongs to.</typeparam>
public abstract class ApplyRegistration<TEvent> : ApplyRegistration where TEvent : ApplyEvent
{
    [ForeignKey(nameof(EventId))]
    public virtual TEvent Event { get; set; } = null!;
}
