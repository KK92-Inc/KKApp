// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Users;

// ============================================================================

/// <summary>
/// Request DTO for creating a new user.
/// </summary>
public record PostUserRequestDTO
{
    /// <summary>
    /// The unique login/username for the user.
    /// </summary>
    [Required, StringLength(100, MinimumLength = 2)]
    public required string Login { get; init; }

    /// <summary>
    /// Optional display name.
    /// </summary>
    [StringLength(100)]
    public string? DisplayName { get; init; }

    /// <summary>
    /// Optional avatar URL.
    /// </summary>
    [Url]
    public string? AvatarUrl { get; init; }
}
