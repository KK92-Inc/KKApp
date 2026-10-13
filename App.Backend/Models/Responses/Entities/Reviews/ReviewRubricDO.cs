// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Reviews;
// ============================================================================

namespace App.Backend.Models.Responses.Entities.Reviews;

/// <summary>
/// The rubric a review was conducted with. The full criteria live on the rubric endpoints.
/// </summary>
public class ReviewRubricDO
{
    public ReviewRubricDO() { }

    public ReviewRubricDO(Rubric rubric)
    {
        Id = rubric.Id;
        Name = rubric.Name;
        Slug = rubric.Slug;
        GitInfoId = rubric.GitInfoId;
    }

    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid GitInfoId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Slug { get; set; } = null!;
}
