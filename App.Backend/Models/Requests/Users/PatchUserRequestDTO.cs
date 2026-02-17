// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Users;

// ============================================================================

/// <summary>
/// Request DTO for updating a user (partial update).
/// </summary>
public record PatchUserRequestDTO
{
    /// <summary>
    /// Optional display name update.
    /// </summary>
    [StringLength(100)]
    public string? DisplayName { get; init; }

    /// <summary>
    /// Optional avatar URL update.
    /// </summary>
    [Url]
    public string? AvatarUrl { get; init; }

    public PatchUserDetailsRequestDTO? Details { get; init; }
}
