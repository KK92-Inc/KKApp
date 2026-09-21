// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Enums;

namespace App.Backend.Models.Requests.Users;

// ============================================================================

/// <summary>
/// Request DTO for creating a new user.
/// </summary>
public record PostUserRequestDTO
{
    [Required, StringLength(7)]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Login can only contain letters, numbers, underscores, and hyphens.")]
    [Description("The user's unique login name.")]
    public required string Login { get; init; }

    [Required, EmailAddress, StringLength(100, MinimumLength = 1)]
    [Description("The user's email address.")]
    public required string Email { get; init; }

    [StringLength(255, MinimumLength = 1)]
    [Description("The user's first name.")]
    public required string FirstName { get; init; }

    [StringLength(255, MinimumLength = 1)]
    [Description("The user's last name.")]
    public required string LastName { get; init; }

    [Url, Description("An optional URL for the user's avatar image.")]
    public string? AvatarUrl { get; init; }

    [Required, Description("The user's role in the application.")]
    public UserRole Role { get; init; }

    [Description("The kickoff to which the user is assigned, if applicable.")]
    public Guid? Kickoff { get; init; }
}
