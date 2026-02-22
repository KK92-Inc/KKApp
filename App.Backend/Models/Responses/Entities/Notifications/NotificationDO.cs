// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Enums;
using App.Backend.Models.Responses;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Notifications;

/// <summary>
/// Notification data object.
/// </summary>
public class NotificationDO : BaseEntityDO<Notification>
{
    public NotificationDO(Notification notification) : base(notification)
    {
        Descriptor = notification.Descriptor;
        ReadAt = notification.ReadAt;
        ResourceId = notification.ResourceId;
        Data = JsonSerializer.Deserialize<NotificationData>(notification.Data)!;
    }

    /// <summary>
    /// Polymorphic data.
    /// </summary>
    [Required]
    public NotificationData Data { get; set; }

    /// <summary>
    /// The notification descriptor outlining what the notification basically is
    /// </summary>
    [Required]
    public NotificationMeta Descriptor { get; set; }

    /// <summary>
    /// Was created at.
    /// </summary>
    [Required]
    public DateTimeOffset? ReadAt { get; set; }

    /// <summary>
    /// Optional reference to a resource this notification is about
    /// </summary>
    [Required]
    public Guid? ResourceId { get; set; }
}
