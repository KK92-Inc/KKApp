// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Comments;

// ============================================================================

/// <summary>
/// Request DTO for creating a new comment.
/// </summary>
public record PostCommentRequestDTO
{
    /// <summary>
    /// The markdown content of the comment.
    /// </summary>
    [Required, StringLength(16384, MinimumLength = 1)]
    public required string Markdown { get; init; }

    /// <summary>
    /// Optional parent comment ID for nested replies.
    /// </summary>
    public Guid? ParentId { get; init; }
}
