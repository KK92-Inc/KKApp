// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Reviews;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Reviews;

/// <summary>
/// Data object representing a rubric for reviews.
/// </summary>
public class RubricDO(Rubric rubric) : BaseEntityDO<Rubric>(rubric)
{
    [Required]
    public string Name { get; set; } = rubric.Name;

    [Required]
    public string Slug { get; set; } = rubric.Slug;

    [Required]
    public bool Public { get; set; } = rubric.Public;

    [Required]
    public bool Enabled { get; set; } = rubric.Enabled;

    [Required]
    public IEnumerable<RubricVariantDO> Variants { get; set; } = rubric.Variants.Select(v => new RubricVariantDO(v));

    [Required]
    public Guid? ProjectId { get; set; } = rubric.ProjectId;

    [Required]
    public UserLightDO Creator { get; set; } = rubric.Creator;

    [Required]
    public GitDO? GitInfo { get; set; } = rubric.GitInfo;

    public static implicit operator RubricDO?(Rubric? rubric) =>
        rubric is null ? null : new(rubric);
}
