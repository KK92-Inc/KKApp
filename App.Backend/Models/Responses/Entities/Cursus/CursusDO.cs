// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursus;

/// <summary>
/// Data object representing a cursus/curriculum.
/// </summary>
public class CursusDO(Domain.Entities.Cursus cursus) : BaseEntityDO<Domain.Entities.Cursus>(cursus)
{
    [Required]
    public string Name { get; set; } = cursus.Name;

    [Required]
    public string Description { get; set; } = cursus.Description;

    [Required]
    public string Slug { get; set; } = cursus.Slug;

    /// <summary>
    /// The track/path of the cursus in .graph format (optional for list views).
    /// </summary>
    public string? Track { get; set; } = cursus.Track?.ToString();

    public static implicit operator CursusDO?(Domain.Entities.Cursus? cursus) =>
        cursus is null ? null : new(cursus);
}
