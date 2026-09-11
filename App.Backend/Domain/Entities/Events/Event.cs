// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Entities.Users;

// ============================================================================

namespace App.Backend.Domain.Entities.Events;

/// <summary>
/// Campus wide events ranging from Hackathons, Meetings, ...
/// </summary>
[Table("tbl_events"), Index(nameof(Name))]
public class Event : BaseEntity
{
    /// <summary>
    /// Name of the event
    /// </summary>
    [Column("name"), MaxLength(255)]
    public string Name { get; set; }

    /// <summary>
    /// A short readable description of the event
    /// </summary>
    [Column("description"), MaxLength(255)]
    public string Description { get; set; }

    /// <summary>
    /// Event thumbnail url
    /// </summary>
    [Column("thumbnail_url"), MaxLength(255)]
    public string? Thumbnail { get; set; }

    /// <summary>
    /// Event body / presentation of what it's about.
    /// </summary>
    [Column("markdown"), MaxLength(2048)]
    public string Markdown { get; set; }

    /// <summary>
    /// The limit / max amount of users that can join this event.
    /// </summary>
    [Column("capacity")]
    public int Capacity { get; set; }

    /// <summary>
    /// The minimum required occupancy for the event to switch from
    /// pending to accepted.
    /// </summary>
    [Column("threshold")]
    public int? Threshold { get; set; }

    /// <summary>
    /// When it will start
    /// </summary>
    [Column("starts_at")]
    public DateTimeOffset StartsAt { get; set; }

    /// <summary>
    /// When it will conclude
    /// </summary>
    [Column("ends_at")]
    public DateTimeOffset EndsAt { get; set; }

    /// <summary>
    /// When will the event stop accepting new users to join and prevent
    /// other users from leaving.
    /// </summary>
    [Column("closes_at")]
    public DateTimeOffset? ClosesAt { get; set; }

    /// <summary>
    /// State of the event, e.g: It's being proposed
    /// </summary>
    [Column("state")]
    public EventState State { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }

    public virtual ICollection<EventFeedback> Feedback { get; set; }
}
