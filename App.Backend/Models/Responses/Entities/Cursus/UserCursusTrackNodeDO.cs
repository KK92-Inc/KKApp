// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursus;

/// <summary>
/// A single node in a user's cursus track, enriched with progression state.
/// </summary>
public class UserCursusTrackNodeDO
{
    [Required]
    public Guid GoalId { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Slug { get; set; } = string.Empty;
    
    /// <summary>
    /// Is the goal currently subscribable at all ?
    /// </summary>
    [Required]
    public bool IsUnlocked { get; set; }

    /// <summary>
    /// The goal's parent / predecessor
    /// </summary>
    public Guid? ParentGoalId { get; set; }
    
    /// <summary>
    /// A UUID that if 2 nodes share means it's a choice between this goal and others that share the id.
    /// Only 1 goal can be selected the way this get's rendered on the UI is a single circle with sub-circles attached to it.
    /// </summary>
    public Guid? ChoiceGroup { get; set; }

    /// <summary>
    /// Null means not yet subscribed.
    /// </summary>
    public EntityObjectState? State { get; set; }
}
