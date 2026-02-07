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
    [Required, MaxLength(200)]
    [Column("title")]
    public required string Title { get; set; }

    [Required, MaxLength(1000)]
    [Column("description")]
    public required string Description { get; set; }

    /// <summary>
    /// Call-to-action button text (e.g., "Learn More", "Register Now")
    /// </summary>
    [Required, MaxLength(50)]
    [Column("action_text")]
    public required string ActionText { get; set; }

    /// <summary>
    /// URL to navigate to when clicking the spotlight
    /// </summary>
    [Required, MaxLength(500)]
    [Column("href")]
    public required string Href { get; set; }

    /// <summary>
    /// Background image URL for the spotlight card
    /// </summary>
    [MaxLength(500)]
    [Column("background_url")]
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
