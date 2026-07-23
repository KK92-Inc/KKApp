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
    [Required, StringLength(255, MinimumLength = 4)]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Login can only contain letters, numbers, underscores, and hyphens.")]
    [Description("The unique login/username for the user.")]
    public required string Login { get; init; }

    /// <summary>
    /// Optional display name.
    /// </summary>
    [Required, EmailAddress, StringLength(100, MinimumLength = 1)]
    [Description("Optional display name for the user.")]
    public required string Email { get; init; }

    /// <summary>
    /// Optional first name of the user.
    /// </summary>
    [StringLength(255, MinimumLength = 1)]
    [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes.")]
    [Description("Optional first name of the user.")]
    public string? FirstName { get; init; }

    /// <summary>
    /// Optional last name of the user.
    /// </summary>
    [StringLength(255, MinimumLength = 1)]
    [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes.")]
    [Description("Optional last name of the user.")]
    public string? LastName { get; init; }

    /// <summary>
    /// Optional avatar URL.
    /// </summary>
    [Url]
    [Description("Optional URL to the user's avatar image.")]
    public string? AvatarUrl { get; init; }
}
