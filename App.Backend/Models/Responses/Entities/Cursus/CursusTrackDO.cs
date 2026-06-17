// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Enums;

namespace App.Backend.Models.Responses.Entities.Cursus;

public class CursusTrackDO
{
    [Required]
    public Guid CursusId { get; set; }
    
    [Required]
    public CursusVariant Variant { get; set; }
    
    [Required]
    public CompletionMode CompletionMode { get; set; }

    public IList<CursusTrackNodeDO> Nodes { get; set; } = [];
}