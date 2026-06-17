// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Relations;
using App.Backend.Models.Responses.Entities.Goals;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursus;

public class CursusTrackNodeDO
{
    [Required]
    public required GoalLightDO Goal { get; set; }
    
    public Guid? ChoiceGroup { get; set; }
    
    public IList<CursusTrackNodeDO> Children { get; set; } = [];
}