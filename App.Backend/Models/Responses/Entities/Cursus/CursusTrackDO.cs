// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Relations;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursus;

/// <summary>
/// Response DTO representing the full track of a cursus.
/// Returns the hierarchical tree of goals.
/// </summary>
public class CursusTrackDO
{
    /// <summary>
    /// The cursus ID this track belongs to.
    /// </summary>
    [Required]
    public Guid CursusId { get; set; }

    /// <summary>
    /// The variant (Fixed or Dynamic) of the cursus.
    /// </summary>
    [Required]
    public CursusVariant Variant { get; set; }

    /// <summary>
    /// How users progress through this track.
    /// </summary>
    [Required]
    public CompletionMode CompletionMode { get; set; }

    /// <summary>
    /// The hierarchical tree of goal nodes.
    /// Only populated for Fixed cursus types.
    /// </summary>
    public IList<CursusTrackNodeDO> Nodes { get; set; } = [];

    /// <summary>
    /// Build a CursusTrackDO from a cursus entity and its goal relations.
    /// </summary>
    public static CursusTrackDO FromRelations(Domain.Entities.Cursus cursus, IEnumerable<CursusGoal> relations)
    {
        return new CursusTrackDO
        {
            CursusId = cursus.Id,
            Variant = cursus.Variant,
            CompletionMode = cursus.CompletionMode,
            Nodes = CursusTrackNodeDO.BuildTree(relations)
        };
    }
}
