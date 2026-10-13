// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Responses.Entities.Cursi;

/// <summary>One goal in a cursus's master track, with its Goal navigation resolved.</summary>
public class CursusTrackNodeDO
{
    [Required]
    public required Guid GoalId { get; set; }
    
    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Description { get; set; }

    [Required]
    public required string Slug { get; set; }

    [Required]
    public Guid? ParentGoalId { get; set; }
}
