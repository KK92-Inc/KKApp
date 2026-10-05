// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Domain.Entities.Users;

/// <summary>
/// A user's participation in a <see cref="Trial"/>.
///
/// This is a row of its own instead of a column on the user because people can take part
/// in more than one trial over time (e.g: retaking one), and because it carries its own
/// state and verdict. A user can only be part of one open trial at a time, see
/// <see cref="UserTrialState"/>.
/// </summary>
[Table("tbl_user_trial")]
[Index(nameof(TrialId), nameof(UserId), IsUnique = true)]
public class UserTrial : BaseEntity
{
    [Column("state")]
    public UserTrialState State { get; set; }

    /// <summary>
    /// The verdict of staff, only meaningful once the trial is over for this user.
    /// </summary>
    [Column("outcome")]
    public TrialOutcome Outcome { get; set; }

    /// <summary>
    /// When the user became active in the trial. Null if they never did.
    /// </summary>
    [Column("started_at")]
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>
    /// When the trial ended for the user, either because they quit or because it ran out.
    /// </summary>
    [Column("ended_at")]
    public DateTimeOffset? EndedAt { get; set; }

    [Column("trial_id")]
    public Guid TrialId { get; set; }

    [ForeignKey(nameof(TrialId))]
    public virtual Trial Trial { get; set; } = null!;

    [Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
