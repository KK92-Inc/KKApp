// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Notifications;

// ============================================================================

/// <summary>
/// Request DTO for creating a notification.
/// </summary>
public record PostNotificationRequestDTO
{
    /// <summary>
    /// The user ID to notify.
    /// </summary>
    [Required]
    public required Guid UserId { get; init; }

    /// <summary>
    /// The notification message.
    /// </summary>
    [Required, StringLength(1024, MinimumLength = 1)]
    public required string Message { get; init; }

    /// <summary>
    /// Optional URL to link the notification to.
    /// </summary>
    [Url]
    public string? Href { get; init; }
}
