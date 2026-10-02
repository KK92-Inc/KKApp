// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Values;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Reviews;

public class ReviewAnnotationDO(Review review, IEnumerable<AnnotationData> annotations)
{
    [Required]
    public Guid ReviewId { get; set; } = review.Id;

    [Required]
    public UserLightDO? Author { get; set; } = review.Reviewer;

    [Required]
    public IEnumerable<AnnotationData> Annotations { get; set; } = annotations;
}
