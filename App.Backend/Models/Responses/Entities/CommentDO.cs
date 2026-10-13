// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

/// <summary>
/// Data object representing a comment on any entity.
/// </summary>
public class CommentDO : BaseEntityDO<Comment>
{
    public CommentDO() { }

    public CommentDO(Comment comment) : base(comment)
    {
        EntityType = comment.EntityType;
        EntityId = comment.EntityId;
        Body = comment.Body;
        User = comment.User;
    }

    [Required]
    public string EntityType { get; set; } = null!;

    [Required]
    public Guid EntityId { get; set; }

    [Required]
    public string Body { get; set; } = null!;

    /// <summary>
    /// The user who authored this comment.
    /// </summary>
    public UserLightDO User { get; set; } = null!;

    public static implicit operator CommentDO?(Comment? comment) =>
        comment is null ? null : new(comment);
}
