// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;
using App.Backend.Domain.Entities.Events;
using App.Backend.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Domain.Relations;

/// <summary>
/// Relation between a user and a event.
/// Basically the list of participants for an Event
/// </summary>
[Table("rel_user_event")]
[PrimaryKey(nameof(UserId), nameof(EventId))]
public class UserEvent : BaseTimestampEntity
{
    [Column("event_id", Order = 0)]
    public Guid EventId { get; set; }

    [Column("user_id", Order = 1)]
    public Guid UserId { get; set; }

    public virtual User User { get; set; }

    public virtual Event Event { get; set; }
}
