// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities;

/// <summary>
/// A trial period is a span of time in which applicants try to complete a cursus, before
/// it is decided whether they get to become a student.
///
/// E/g: You run a 2 week trial in August. Applicants pick it, their account is switched on
/// when it starts and switched off again when it ends. Afterwards staff select who moves on.
///
/// Trials are optional and independent from <see cref="Kickoff"/>s: a school that doesn't
/// want trials never creates one, and nothing else in the system requires one to exist.
/// </summary>
[Table("tbl_trials")]
public class Trial : BaseEntity
{
    /// <summary>
    /// A name for the trial e.g: Piscine::2026::August
    /// </summary>
    [Column("name"), StringLength(255)]
    public required string Name { get; set; }

    /// <summary>
    /// The limit / max amount of users that can take part in this trial.
    /// </summary>
    [Column("capacity")]
    public int Capacity { get; set; }

    /// <summary>
    /// When the trial is scheduled to begin.
    /// </summary>
    [Column("starts_at")]
    public DateTimeOffset StartsAt { get; set; }

    /// <summary>
    /// When the trial is scheduled to end.
    /// </summary>
    [Column("ends_at")]
    public DateTimeOffset EndsAt { get; set; }

    /// <summary>
    /// When the trial actually began, i.e. when its participants were queued for activation.
    /// Null while it is still upcoming. Owned by the trial job, never by a caller.
    /// </summary>
    [Column("started_at")]
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>
    /// When the trial actually ended, i.e. when its participants were wrapped up.
    /// Null until then. Owned by the trial job, never by a caller.
    /// </summary>
    [Column("ended_at")]
    public DateTimeOffset? EndedAt { get; set; }

    /// <summary>
    /// The cursus participants work on during the trial, they get subscribed to it when they start.
    /// </summary>
    [Column("cursus_id")]
    public Guid CursusId { get; set; }

    [ForeignKey(nameof(CursusId))]
    public virtual Cursus Cursus { get; set; } = null!;
}
