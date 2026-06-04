// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
    [Description("Indicates whether the notification has been read.")]
    public bool? IsRead { get; init; }
}
