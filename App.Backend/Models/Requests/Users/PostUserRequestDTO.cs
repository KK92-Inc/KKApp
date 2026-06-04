// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
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
    [Description("The unique login/username for the user.")]
    public required string Login { get; init; }

    /// <summary>
    /// Optional display name.
    /// </summary>
    [StringLength(100, MinimumLength = 1)]
    [Description("Optional display name for the user.")]
    public string? DisplayName { get; init; }

    /// <summary>
    /// Optional avatar URL.
    /// </summary>
    [Url]
    [Description("Optional URL to the user's avatar image.")]
    public string? AvatarUrl { get; init; }
}
