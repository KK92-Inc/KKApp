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
public class RubricLightDO(Rubric rubric) : BaseEntityDO<Rubric>(rubric)
{
    [Required]
    public string Name { get; set; } = rubric.Name;

    [Required]
    public string Slug { get; set; } = rubric.Slug;

    [Required]
    public bool Public { get; set; } = rubric.Public;

    [Required]
    public bool Enabled { get; set; } = rubric.Enabled;

    public IEnumerable<RubricVariantDO> Variants { get; set; } = rubric.Variants.Select(v => new RubricVariantDO(v));

    public static implicit operator RubricLightDO?(Rubric? rubric) =>
        rubric is null ? null : new(rubric);
}
