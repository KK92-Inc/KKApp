// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;
using App.Backend.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Domain.Entities;


/// <summary>
/// Represents a textual comment attached to another entity in the system.
/// </summary>
/// <remarks>
/// Comments are stored in the <c>tbl_comment</c> table and are intended to be
/// a lightweight, polymorphic way to attach user-entered content to other
/// entities (for example: reviews, posts, or other domain objects). The
/// EntityType/EntityId pair identifies which entity the comment belongs to.
///
/// The entity supports basic hierarchical threading via <see cref="BaseEntity.ParentId"/>,
/// and is indexed by both <see cref="BaseEntity.ParentId"/> and <see cref="UserId"/> to
/// improve common query patterns (fetch by parent, fetch by user).
/// </remarks>
[Table("tbl_comment")]
[Index(nameof(EntityId)), Index(nameof(UserId))]
public class Comment : BaseEntity
{
    /// <summary>
    /// The name of the entity type this comment targets. This value is intended
    /// to be the literal type name used by the application (for example: "Review").
    /// </summary>
    [Column("entity_type")]
    public required string EntityType { get; set; }

    [Column("entity_id")]
    public Guid EntityId { get; set; }

    [Column("body")]
    public required string Body { get; set; }

    /// <summary>
    /// The identifier of the user who authored the comment.
    /// </summary>
    [Column("user_id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user that authored this comment.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
