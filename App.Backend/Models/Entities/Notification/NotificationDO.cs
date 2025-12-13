// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Enums;

namespace App.Backend.Models.Entities;

public class NotificationDO : BaseEntityDO<Notification>
{
    public NotificationDO(Notification notification) : base(notification)
    {
        Descriptor = notification.Descriptor;
        ReadAt = notification.ReadAt;
        ResourceId = notification.ResourceId;
        Data = NotificationDataDO.FromJson(notification.Data, notification.Descriptor);
    }

    /// <summary>
    /// Polymorphic data based on the descriptor.
    /// </summary>
    public NotificationDataDO? Data { get; set; }

    [Required]
    public NotifiableVariant Descriptor { get; set; }

    /// <summary>
    /// Was created at.
    /// </summary>
    public DateTimeOffset? ReadAt { get; set; }

    /// <summary>
    /// Optional reference to a resource this notification is about
    /// </summary>
    public Guid? ResourceId { get; set; }
}
