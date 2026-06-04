// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
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
    [Description("The title of the spotlight.")]
    public string? Title { get; init; }

    /// <summary>
    /// Optional description update.
    /// </summary>
    [StringLength(16384, MinimumLength = 1)]
    [Description("The description/body of the spotlight.")]
    public string? Description { get; init; }

    /// <summary>
    /// Optional call-to-action button text update.
    /// </summary>
    [StringLength(64, MinimumLength = 1)]
    [Description("Optional call-to-action button text.")]
    public string? ActionText { get; init; }

    /// <summary>
    /// Optional call-to-action URL update.
    /// </summary>
    [Url]
    [Description("Optional URL for the call-to-action button.")]
    public string? Href { get; init; }

    /// <summary>
    /// Optional background image URL update.
    /// </summary>
    [Url]
    [Description("Optional background image URL.")]
    public string? BackgroundUrl { get; init; }

    /// <summary>
    /// Optional starts at update.
    /// </summary>
    [Description("Optional start date/time for the spotlight.")]
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
