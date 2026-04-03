// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities;

/// <summary>
/// Universal membership record. One row = one user's relationship to one entity.
///
/// Uses a polymorphic association: entity_type + entity_id together identify
/// the target. There is no FK constraint on entity_id — referential integrity
/// is enforced at the application layer.
/// </summary>
[Table("tbl_members")]
[Index(nameof(GitId),      nameof(UserId))]
public class Member : BaseEntity
{
    /// <summary>
    /// Discriminates which table entity_id refers to.
    /// </summary>
    [Column("entity_type")]
    public MemberEntityType EntityType { get; set; }

    [Column("entity_id")]
    public Guid EntityId { get; set; }

    [Column("git_id")]
    public Guid? GitId { get; set; }

    [ForeignKey(nameof(GitId))]
    public virtual Git? Git { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }

    [Column("role")]
    public MemberRole Role { get; set; }

    [Column("left_at")]
    public DateTimeOffset? LeftAt { get; set; }
}
