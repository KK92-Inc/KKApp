// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursi;

public class CursusDO(Cursus cursus) : BaseEntityDO<Cursus>(cursus)
{
    public string Name { get; set; } = cursus.Name;

    public string Description { get; set; } = cursus.Description;

    public string Slug { get; set; } = cursus.Slug;

    public string? Thumbnail { get; set; } = cursus.Thumbnail;

    public bool Enabled { get; set; } = cursus.Enabled;

    public bool Public { get; set; } = cursus.Public;

    public bool Deprecated { get; set; } = cursus.Deprecated;

    public CursusVariant Variant { get; set; } = cursus.Variant;

    public CursusMode Mode { get; set; } = cursus.Mode;
    public WorkspaceDO Workspace { get; set; } = cursus.Workspace;

    public static implicit operator CursusDO?(Cursus? cursus) => cursus is null ? null : new(cursus);
}
