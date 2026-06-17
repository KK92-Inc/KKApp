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
    
    [Required]
    public bool IsUnlocked { get; set; }

    public Guid? ParentGoalId { get; set; }
    
    public Guid? ChoiceGroup { get; set; }

    /// <summary>
    /// Null means not yet subscribed.
    /// </summary>
    public EntityObjectState? State { get; set; }
}
