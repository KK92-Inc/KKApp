// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Cursus;

// ============================================================================

/// <summary>
/// A single node in the flat representation of a cursus track.
/// </summary>
public record CursusTrackNodeDTO
{
    /// <summary>
    /// The goal ID this node represents.
    /// </summary>
    [Required]
    public required Guid GoalId { get; init; }

    /// <summary>
    /// The parent goal ID within this cursus track.
    /// Null for root-level goals.
    /// </summary>
    public Guid? ParentId { get; init; }

    /// <summary>
    /// Optional choice group identifier. Siblings sharing the same non-null
    /// value are alternatives — the user must complete at least one from
    /// the group. Null means the goal is required.
    /// </summary>
    public Guid? Group { get; init; }
}

/// <summary>
/// Request DTO for setting or replacing the track of a cursus.
/// Accepts a flat list of nodes that form a tree via parent references.
/// </summary>
public record PostCursusTrackRequestDTO
{
    /// <summary>
    /// The flat list of track nodes. Each node references its parent
    /// to form the hierarchy. Root nodes have no parent.
    /// </summary>
    [Required, MinLength(1)]
    public required IList<CursusTrackNodeDTO> Nodes { get; init; }
}
