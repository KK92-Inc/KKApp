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

public class ReviewAnnotationDO
{
    public ReviewAnnotationDO() { }

    public ReviewAnnotationDO(Review review, IEnumerable<AnnotationData> annotations)
    {
        ReviewId = review.Id;
        Author = review.Reviewer;
        Annotations = annotations;
    }

    [Required]
    public Guid ReviewId { get; set; }

    [Required]
    public UserBriefDO? Author { get; set; }

    [Required]
    public IEnumerable<AnnotationData> Annotations { get; set; } = [];
}
