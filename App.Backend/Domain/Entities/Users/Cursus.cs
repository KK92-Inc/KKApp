// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain;
using App.Backend.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities.Users;

/// <summary>
/// Cursus entity representing the association between users and cursus.
/// </summary>
[Table("tbl_user_cursus")]
public class UserCursus : BaseEntity
{
    public UserCursus()
    {
        State = EntityObjectState.Active;
        UnlocksAt = null;

        UserId = Guid.Empty;
        User = null!;

        CursusId = Guid.Empty;
        Cursus = null!;
    }

    [Column("state")]
    public EntityObjectState State { get; set; }

    /// <summary>
    /// If set, locks modifications on the entity until the specified time.
    /// E.g: Used for cooldowns after unsubscribing.
    /// </summary>
    [Column("unlocks_at")]
    public DateTimeOffset? UnlocksAt { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }

    [Column("cursus_id")]
    public Guid CursusId { get; set; }

    [ForeignKey(nameof(CursusId))]
    public virtual Cursus Cursus { get; set; }
}
