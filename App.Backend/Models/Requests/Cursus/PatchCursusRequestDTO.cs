// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Cursus;

public class PatchCursusRequestDTO
{
    [StringLength(256, MinimumLength = 1)]
    [Description("The name of cursus")]
    public string? Name { get; init; }

    [ StringLength(16384, MinimumLength = 1)]
    [Description("Description of the cursus.")]
    public string? Description { get; init; }

    [Description("Indicates whether the cursus can be subscribed to.")]
    public bool? Enabled { get; init; }

    [Description("Indicates whether the cursus is publicly visible.")]
    public bool? Public { get; init; }

    [Description("The cursus track")]
    public PutCursusTrackRequestDTO? Track { get; init; }
}
