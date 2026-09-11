// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace App.Backend.Domain.Entities;

// ============================================================================

/// <summary>
/// Platform-wide spotlight announcements (events, campaigns, etc.).
/// These are NOT stored in the notifications table - they're their own entity
/// but can be projected/returned as NotificationDO for API consistency.
/// </summary>
[Table("tbl_spotlights")]
[Index(nameof(StartsAt), nameof(EndsAt))]
public class Spotlight : BaseEntity
{
    [Column("title"), MaxLength(255)]
    public required string Title { get; set; }

    [Column("description"), MaxLength(255)]
    public required string Description { get; set; }

    /// <summary>
    /// Call-to-action button text (e.g., "Learn More", "Register Now")
    /// </summary>
    [Column("action_text"), MaxLength(50)]
    public required string ActionText { get; set; }

    /// <summary>
    /// URL to navigate to when clicking the spotlight
    /// </summary>
    [Column("href"), MaxLength(255)]
    public required string Href { get; set; }

    /// <summary>
    /// Background image URL for the spotlight card
    /// </summary>
    [Column("background_url"), MaxLength(255)]
    public string? BackgroundUrl { get; set; }

    /// <summary>
    /// When the spotlight becomes visible
    /// </summary>
    [Column("starts_at")]
    public DateTimeOffset StartsAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// When the spotlight expires (null = never expires, must be manually disabled)
    /// </summary>
    [Column("ends_at")]
    public DateTimeOffset? EndsAt { get; set; }

    /// <summary>
    /// Whether this spotlight is currently active
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Users who have dismissed this spotlight
    /// </summary>
    public virtual ICollection<SpotlightDismissal> Dismissals { get; set; } = [];

    /// <summary>
    /// Check if this spotlight should be shown right now
    /// </summary>
    [NotMapped]
    public bool IsVisible => IsActive
        && StartsAt <= DateTimeOffset.UtcNow
        && (EndsAt == null || EndsAt > DateTimeOffset.UtcNow);
}
