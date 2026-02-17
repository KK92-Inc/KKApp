// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

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
    // public string? Email { get; init; }

    [StringLength(16384)]
    public string? Markdown { get; init; }

    [StringLength(100)]
    public string? FirstName { get; init; }

    [StringLength(100)]
    public string? LastName { get; init; }

    /// <summary>
    /// Notification preferences flags.
    /// </summary>
    public NotificationMeta? EnabledNotifications { get; init; }

    [Url]
    public string? GithubUrl { get; init; }

    [Url]
    public string? LinkedinUrl { get; init; }

    [Url]
    public string? RedditUrl { get; init; }

    [Url]
    public string? WebsiteUrl { get; init; }
}
