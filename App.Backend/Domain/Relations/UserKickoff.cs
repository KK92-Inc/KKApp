// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Events;
using App.Backend.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Domain.Relations;

/// <summary>
/// Relation between a user and a kickoff.
/// This effectively describes a "cohort"
/// </summary>
[Table("rel_user_kickoff")]
[PrimaryKey(nameof(UserId), nameof(KickoffId))]
public class UserKickoff : BaseTimestampEntity
{
    [Column("kickoff_id", Order = 0)]
    public Guid KickoffId { get; set; }

    [Column("user_id", Order = 1)]
    public Guid UserId { get; set; }

    /// <summary>
    /// When the kick off entry was processed for this specific user.
    /// </summary>
    [Column("processed_at")]
    public DateTimeOffset? ProcessedAt { get; set; }

    public virtual User User { get; set; }

    public virtual Kickoff Kickoff { get; set; }
}
