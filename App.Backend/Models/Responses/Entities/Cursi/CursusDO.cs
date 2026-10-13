// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursi;

public class CursusDO : BaseEntityDO<Cursus>
{
    public CursusDO() { }

    public CursusDO(Cursus cursus) : base(cursus)
    {
        Name = cursus.Name;
        Description = cursus.Description;
        Slug = cursus.Slug;
        Thumbnail = cursus.Thumbnail;
        Enabled = cursus.Enabled;
        Public = cursus.Public;
        Deprecated = cursus.Deprecated;
        Variant = cursus.Variant;
        Mode = cursus.Mode;
        Workspace = cursus.Workspace;
    }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    [Required]
    public string Slug { get; set; } = null!;

    [Required]
    public string? Thumbnail { get; set; }

    [Required]
    public bool Enabled { get; set; }

    [Required]
    public bool Public { get; set; }

    [Required]
    public bool Deprecated { get; set; }

    [Required]
    public CursusVariant Variant { get; set; }

    [Required]
    public CursusMode Mode { get; set; }
    
    [Required]
    public WorkspaceDO Workspace { get; set; } = null!;

    public static implicit operator CursusDO?(Cursus? cursus) => cursus is null ? null : new(cursus);
}
