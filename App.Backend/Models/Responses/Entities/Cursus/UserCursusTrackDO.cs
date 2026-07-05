// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Relations;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursus;

public class UserCursusTrackDO
{
    [Required]
    public Guid CursusId { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public CompletionMode CompletionMode { get; set; }

    [Required]
    public IList<UserCursusTrackNodeDO> Nodes { get; set; } = [];
}