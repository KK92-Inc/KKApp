// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Models.Requests.Notifications;

// ============================================================================

/// <summary>
/// Request DTO for marking a notification as read.
/// </summary>
public record PatchNotificationRequestDTO
{
    /// <summary>
    /// Whether the notification has been read.
    /// </summary>
    public bool? IsRead { get; init; }
}
