// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
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
    [StringLength(100, MinimumLength = 1)]
    [Description("The display name for the user.")]
    public string? DisplayName { get; init; }

    /// <summary>
    /// Optional avatar URL update.
    /// </summary>
    [Url]
    [Description("URL to the user's avatar image.")]
    public string? AvatarUrl { get; init; }

    [Description("Additional user details.")]
    public PatchUserDetailsRequestDTO? Details { get; init; }
}
