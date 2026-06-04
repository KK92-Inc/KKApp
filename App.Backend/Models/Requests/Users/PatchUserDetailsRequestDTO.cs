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
/// Request DTO for updating user details (partial update).
/// </summary>
public record PatchUserDetailsRequestDTO
{
    // [EmailAddress]
    // [Description("The user's email address.")]
    // public string? Email { get; init; }

    [StringLength(16384)]
    [Description("Optional markdown biography or about text.")]
    public string? Markdown { get; init; }

    [StringLength(100, MinimumLength = 1)]
    [Description("The user's first name.")]
    public string? FirstName { get; init; }

    [StringLength(100, MinimumLength = 1)]
    [Description("The user's last name.")]
    public string? LastName { get; init; }

    /// <summary>
    /// Notification preferences flags.
    /// </summary>
    [Description("Notification preferences flags.")]
    public NotificationMeta? EnabledNotifications { get; init; }

    [Url]
    [Description("Optional GitHub profile URL.")]
    public string? GithubUrl { get; init; }

    [Url]
    [Description("Optional LinkedIn profile URL.")]
    public string? LinkedinUrl { get; init; }

    [Url]
    [Description("Optional Reddit profile URL.")]
    public string? RedditUrl { get; init; }

    [Url]
    [Description("Optional personal website URL.")]
    public string? WebsiteUrl { get; init; }
}
