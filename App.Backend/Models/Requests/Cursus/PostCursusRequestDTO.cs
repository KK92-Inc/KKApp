// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Enums;
using App.Backend.Models.Responses.Entities.Cursus;

namespace App.Backend.Models.Requests.Cursus;

// ============================================================================

public class PostCursusRequestDTO
{
    [Required, StringLength(256, MinimumLength = 1)]
    public string Name { get; init; }

    [Required, StringLength(16384)]
    [Description("Optional description of the cursus.")]
    public string Description { get; init; }

    [Required, Description("Indicates whether the cursus is currently active.")]
    public bool Active { get; init; }

    [Required, Description("Indicates whether the cursus is publicly visible.")]
    public bool Public { get; init; }

    [Required, MinLength(1)]
    [Description("The flat list of track nodes forming the cursus hierarchy.")]
    public required IList<CursusTrackNodeDO> Nodes { get; init; }

    [Required, Description("The cursus variant: Static (fixed track) or Dynamic (free-roam).")]
    public CursusVariant Variant { get; init; }

    [Required, Description("How users progress through the track: Ring (level-by-level) or FreeStyle (branch-independent).")]
    public CompletionMode CompletionMode { get; init; }
}
