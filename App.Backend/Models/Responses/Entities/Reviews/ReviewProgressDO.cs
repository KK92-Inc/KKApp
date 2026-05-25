// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Reviews;

/// <summary>
/// Data object representing the progress of reviews for a user project and rubric.
/// This includes the overall completion status and the breakdown of review variants.
/// </summary>
public class ReviewVariantProgressDO
{
    [Required]
    public ReviewKinds Kind { get; init; }
    
    [Required]
    public int Required { get; init; }

    [Required]
    public int Finished { get; init; }

    [Required]
    public int Active { get; init; }
}

public class ReviewProgressDO(Rubric rubric)
{
    [Required]
    public RubricLightDO Rubric { get; init; } = rubric;

    [Required]
    public IEnumerable<ReviewVariantProgressDO> Variants { get; init; } = [];
}
