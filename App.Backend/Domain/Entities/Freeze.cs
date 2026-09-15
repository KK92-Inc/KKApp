// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Backend.Domain.Entities.Users;

// ============================================================================

namespace App.Backend.Domain.Entities;

/// <summary>
/// A freeze is a temporary suspension on a account for a given reason.
/// Say for example your studies have to be paused for 1 month because
/// something grave has happened or whatever.
/// </summary>
[Table("tbl_freeze")]
public class Freeze : BaseEntity
{
    /// <summary>
    /// The reason for suspension
    /// </summary>
    [Column("name"), StringLength(2048)]
    public required string Reason { get; set; }

    /// <summary>
    /// The scheduled date on when to invoke this kickoff.
    /// </summary>
    [Column("starts_at")]
    public DateTimeOffset StartsAt { get; set; }

    /// <summary>
    /// The scheduled date on when to invoke this kickoff.
    /// </summary>
    [Column("ends_at")]
    public required DateTimeOffset EndsAt { get; set; }

    [Column("user_id")]
    public required Guid UserId { get; set; }

    public virtual User User { get; set; }
}
