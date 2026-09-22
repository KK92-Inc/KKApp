// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursi;

public class CursusDO(Cursus cursus) : BaseEntityDO<Cursus>(cursus)
{
    [Required]
    public string Name { get; set; } = cursus.Name;

    [Required]
    public string Description { get; set; } = cursus.Description;

    [Required]
    public string Slug { get; set; } = cursus.Slug;

    [Required]
    public string? Thumbnail { get; set; } = cursus.Thumbnail;

    [Required]
    public bool Enabled { get; set; } = cursus.Enabled;

    [Required]
    public bool Public { get; set; } = cursus.Public;

    [Required]
    public bool Deprecated { get; set; } = cursus.Deprecated;

    [Required]
    public CursusVariant Variant { get; set; } = cursus.Variant;

    [Required]
    public CursusMode Mode { get; set; } = cursus.Mode;
    
    [Required]
    public WorkspaceDO Workspace { get; set; } = cursus.Workspace;

    public static implicit operator CursusDO?(Cursus? cursus) => cursus is null ? null : new(cursus);
}
