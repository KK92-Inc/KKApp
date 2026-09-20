// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Requests.Cursus;

/// <summary>
/// Create a cursus.
/// </summary>
public class PostCursusRequestDTO
{
    [Required, StringLength(256, MinimumLength = 1)]
    [Description("The name of cursus")]
    public required string Name { get; init; }

    [Required, StringLength(16384, MinimumLength = 1)]
    [Description("Description of the cursus.")]
    public required string Description { get; init; }

    [Required]
    [Description("Indicates whether the cursus can be subscribed to.")]
    public bool Enabled { get; init; }

    [Required]
    [Description("Indicates whether the cursus is publicly visible.")]
    public bool Public { get; init; }

    [Required]
    [Description("What kind of cursus this is.")]
    public CursusVariant Variant { get; init; }

    [Required]
    [Description("Defines the type of progression to be made in the cursus.")]
    public CursusMode Mode { get; init; }

    [Description("The cursus track, if you want to set it at creation time. Otherwise use PUT /cursus/{id}/track once the goals it references exist.")]
    public PutCursusTrackRequestDTO? Track { get; init; }
}
