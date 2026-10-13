// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Reviews;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Reviews;

/// <summary>
/// Data object representing a rubric for reviews.
/// </summary>
public class RubricLightDO : BaseEntityDO<Rubric>
{
    public RubricLightDO() { }

    public RubricLightDO(Rubric rubric) : base(rubric)
    {
        Name = rubric.Name;
        Slug = rubric.Slug;
        Public = rubric.Public;
        Enabled = rubric.Enabled;
        Variants = rubric.Variants.Select(v => new RubricVariantDO(v));
    }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Slug { get; set; } = null!;

    [Required]
    public bool Public { get; set; }

    [Required]
    public bool Enabled { get; set; }

    public IEnumerable<RubricVariantDO> Variants { get; set; } = [];

    public static implicit operator RubricLightDO?(Rubric? rubric) =>
        rubric is null ? null : new(rubric);
}
