// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Enums;

namespace App.Backend.Models.Requests.Cursus;

// ============================================================================

/// <summary>
/// Request DTO for creating a new cursus.
/// </summary>
public record PostCursusRequestDTO
{
    /// <summary>
    /// The name of the cursus.
    /// </summary>
    [Required, StringLength(256, MinimumLength = 1)]
    public required string Name { get; init; }

    // /// <summary>
    // /// The unique slug for the cursus.
    // /// </summary>
    // [Required, StringLength(256, MinimumLength = 1)]
    // [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$",
    //     ErrorMessage = "Slug must be lowercase alphanumeric with hyphens only")]
    // public required string Slug { get; init; }

    /// <summary>
    /// Optional description of the cursus.
    /// </summary>
    [Required, StringLength(16384)]
    [Description("Optional description of the cursus.")]
    public required string Description { get; init; }

    /// <summary>
    /// Whether the cursus is active.
    /// </summary>
    [Description("Indicates whether the cursus is currently active.")]
    public bool Active { get; init; } = true;

    /// <summary>
    /// Whether the project is public.
    /// </summary>
    [Description("Indicates whether the cursus is publicly visible.")]
    public bool Public { get; init; } = false;

    /// <summary>
    /// The cursus variant (Fixed track or Dynamic free-roam).
    /// Defaults to Fixed.
    /// </summary>
    [Description("The cursus variant: Static (fixed track) or Dynamic (free-roam).")]
    public CursusVariant Variant { get; init; } = CursusVariant.Static;

    /// <summary>
    /// How users progress through the track: level-by-level (Ring) or
    /// branch-independent (FreeStyle). Defaults to Ring.
    /// </summary>
    [Description("How users progress through the track: Ring (level-by-level) or FreeStyle (branch-independent).")]
    public CompletionMode CompletionMode { get; init; } = CompletionMode.Ring;
}
