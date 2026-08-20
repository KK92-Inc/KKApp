// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Requests.Cursus;

/// <summary>
/// Request DTO for setting or replacing the track of a cursus.
/// Accepts a flat list of nodes that form a tree via parent references.
/// </summary>
public class PostCursusTrackRequestDTO
{
    /// <summary>
    /// The flat list of track nodes. Each node references its parent
    /// to form the hierarchy. Root nodes have no parent.
    /// </summary>
    [Required, MinLength(1)]
    [Description("The flat list of track nodes forming the cursus hierarchy.")]
    public required IList<CursusTrackNodeDO> Nodes { get; init; }
}
