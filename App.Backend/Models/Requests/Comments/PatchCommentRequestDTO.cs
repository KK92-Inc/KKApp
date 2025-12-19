// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Comments;

// ============================================================================

/// <summary>
/// Request DTO for updating a comment (partial update).
/// </summary>
public record PatchCommentRequestDTO
{
    /// <summary>
    /// The updated markdown content.
    /// </summary>
    [StringLength(16384, MinimumLength = 1)]
    public string? Markdown { get; init; }
}
