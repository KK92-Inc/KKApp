// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Cursus;

// ============================================================================

/// <summary>
/// A single node in the flat representation of a cursus track.
/// </summary>
public class CursusTrackNodeDO
{
    /// <summary>
    /// The goal ID this node represents.
    /// </summary>
    [Required]
    [Description("The goal ID this node represents.")]
    public required Guid GoalId { get; init; }

    /// <summary>
    /// The parent goal ID within this cursus track.
    /// Null for root-level goals.
    /// </summary>
    [Description("The parent goal ID within this cursus track. Null for root-level goals.")]
    public Guid? ParentId { get; init; }
}
