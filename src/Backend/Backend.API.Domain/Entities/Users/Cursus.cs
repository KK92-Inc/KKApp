// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Backend.API.Domain;
using Backend.API.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace Backend.API.Domain.Entities.Users;

/// <summary>
/// Cursus entity representing the association between users and cursus.
/// </summary>
[Table("tbl_user_cursus")]
public class UserCursus : BaseEntity
{
    public UserCursus()
    {
        State = EntityState.Active;

        UserId = Guid.Empty;
        User = null!;

        CursusId = Guid.Empty;
        Cursus = null!;
    }

    [Column("state")]
    public EntityState State { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }

    [Column("cursus_id")]
    public Guid CursusId { get; set; }

    [ForeignKey(nameof(CursusId))]
    public virtual Cursus Cursus { get; set; }

    /// <summary>
    /// The track / path of the Cursus stored in the .graph format.
    /// </summary>
    [Column("track", TypeName = "jsonb")]
    public string? Track { get; set; }
}
