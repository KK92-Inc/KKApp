// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using App.Backend.Domain.Enums;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Domain.Entities;


[Table("tbl_notifications")]
[Index(nameof(NotifiableId), nameof(ReadAt))]
public class Notification : BaseEntity
{
    public Notification()
    {
        ReadAt = null;
        Descriptor = default;
        ResourceId = null;
    }

	/// <summary>
    /// The notification descriptor outlining what the notification basically is
    /// </summary>
    [Column("descriptor")]
    public NotificationMeta Descriptor { get; set; }


    /// <summary>
    /// When the notification was read
    /// </summary>
    [Column("read_at")]
    public DateTimeOffset? ReadAt { get; set; }

    /// <summary>
    /// The entity to be notified.
    /// </summary>
    [Column("notifiable_id")]
    public Guid NotifiableId { get; set; }

    /// <summary>
    /// This notification targets a specific resource as context.
    /// </summary>
    [Column("resource_id")]
    public Guid? ResourceId { get; set; }

    /// <summary>
    /// The notification data.
    /// </summary>
    [Column("data", TypeName = "json"), Required]
    public required string Data { get; set; }
}
