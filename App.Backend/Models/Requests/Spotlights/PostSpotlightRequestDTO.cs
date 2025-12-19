// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Spotlights;

// ============================================================================

/// <summary>
/// Request DTO for creating a new spotlight (platform-wide announcement).
/// </summary>
public record PostSpotlightRequestDTO
{
    /// <summary>
    /// The title of the spotlight.
    /// </summary>
    [Required, StringLength(256, MinimumLength = 1)]
    public required string Title { get; init; }

    /// <summary>
    /// The description/body of the spotlight.
    /// </summary>
    [Required, StringLength(16384, MinimumLength = 1)]
    public required string Description { get; init; }

    /// <summary>
    /// Optional call-to-action button text.
    /// </summary>
    [StringLength(64)]
    public string? ActionText { get; init; }

    /// <summary>
    /// Optional URL for the call-to-action button.
    /// </summary>
    [Url]
    public string? Href { get; init; }

    /// <summary>
    /// Optional background image URL.
    /// </summary>
    [Url]
    public string? BackgroundUrl { get; init; }

    /// <summary>
    /// When the spotlight should start being displayed.
    /// </summary>
    public DateTimeOffset? StartsAt { get; init; }

    /// <summary>
    /// When the spotlight should stop being displayed.
    /// </summary>
    public DateTimeOffset? EndsAt { get; init; }

    /// <summary>
    /// Whether the spotlight is currently active.
    /// </summary>
    public bool IsActive { get; init; } = true;
}
