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
public class CommentDO(Comment comment) : BaseEntityDO<Comment>(comment)
{
    [Required]
    public string EntityType { get; set; } = comment.EntityType;

    [Required]
    public Guid EntityId { get; set; } = comment.EntityId;

    [Required]
    public string Body { get; set; } = comment.Body;

    [Required]
    public Guid UserId { get; set; } = comment.UserId;

    /// <summary>
    /// The user who authored this comment.
    /// </summary>
    public UserLightDO? User { get; set; } = comment.User;

    public static implicit operator CommentDO?(Comment? comment) =>
        comment is null ? null : new(comment);
}
