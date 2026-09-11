// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities.Events;

/// <summary>
/// Campus wide events ranging from Hackathons, Meetings, ...
/// </summary>
[Table("tbl_event_feedback")]
public class EventFeedback : BaseEntity
{
    /// <summary>
    /// A rating from 0-10 on how to the event was.
    /// </summary>
    [Column("rating")]
    public int Rating { get; set; }

    /// <summary>
    /// The comment the user left.
    /// </summary>
    [Column("comment_id")]
    public Guid CommentId { get; set; }

    /// <summary>
    /// The event this feedback relates to.
    /// </summary>
    [Column("event_id")]
    public Guid EventId { get; set; }

    [ForeignKey(nameof(EventId))]
    public virtual Event Event { get; set; }    
}
