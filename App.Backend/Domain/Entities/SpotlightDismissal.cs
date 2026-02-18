// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;
using App.Backend.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace App.Backend.Domain.Entities;

// ============================================================================

/// <summary>
/// Tracks which users have dismissed which spotlights.
/// TODO: Migrate to rel as it is basically a join table
/// </summary>
[Table("tbl_spotlight_dismissals")]
[PrimaryKey(nameof(UserId), nameof(SpotlightId))]
public class SpotlightDismissal
{
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("spotlight_id")]
    public Guid SpotlightId { get; set; }

    [Column("dismissed_at")]
    public DateTimeOffset DismissedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual Spotlight Spotlight { get; set; } = null!;
}
