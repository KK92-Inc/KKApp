// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Reviews;
using App.Backend.Models.Requests;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Reviews;

/// <summary>
/// Data object representing a rubric for reviews.
/// </summary>
public class RubricDO : BaseEntityDO<Rubric>
{
    public RubricDO() { }

    public RubricDO(Rubric rubric) : base(rubric)
    {
        Name = rubric.Name;
        Slug = rubric.Slug;
        Public = rubric.Public;
        Enabled = rubric.Enabled;
        Deprecated = rubric.Deprecated;
        Variants = rubric.Variants.Select(v => new RubricVariantDO(v));
        ProjectId = rubric.ProjectId;
        GitInfo = rubric.GitInfo;
    }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Slug { get; set; } = null!;

    [Required]
    public bool Public { get; set; }

    [Required]
    public bool Enabled { get; set; }

    [Required]
    public bool Deprecated { get; set; }

    [Required]
    public IEnumerable<RubricVariantDO> Variants { get; set; } = [];

    [Required]
    public Guid? ProjectId { get; set; }

    [Required]
    public GitDO GitInfo { get; set; } = null!;

    public static implicit operator RubricDO?(Rubric? rubric) =>
        rubric is null ? null : new(rubric);
}
