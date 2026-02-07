// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Spotlights;

// ============================================================================

/// <summary>
/// Request DTO for updating a spotlight (partial update).
/// </summary>
public record PatchSpotlightRequestDTO
{
    /// <summary>
    /// Optional title update.
    /// </summary>
    [StringLength(256, MinimumLength = 1)]
    public string? Title { get; init; }

    /// <summary>
    /// Optional description update.
    /// </summary>
    [StringLength(16384, MinimumLength = 1)]
    public string? Description { get; init; }

    /// <summary>
    /// Optional call-to-action button text update.
    /// </summary>
    [StringLength(64)]
    public string? ActionText { get; init; }

    /// <summary>
    /// Optional call-to-action URL update.
    /// </summary>
    [Url]
    public string? Href { get; init; }

    /// <summary>
    /// Optional background image URL update.
    /// </summary>
    [Url]
    public string? BackgroundUrl { get; init; }

    /// <summary>
    /// Optional starts at update.
    /// </summary>
    public DateTimeOffset? StartsAt { get; init; }

    /// <summary>
    /// Optional ends at update.
    /// </summary>
    public DateTimeOffset? EndsAt { get; init; }

    /// <summary>
    /// Optional active status update.
    /// </summary>
    public bool? IsActive { get; init; }
}
