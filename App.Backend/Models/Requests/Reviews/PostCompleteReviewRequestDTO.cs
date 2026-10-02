// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Values;
using App.Backend.Models.Responses.Entities.Reviews;

namespace App.Backend.Models.Requests.Reviews;

// ============================================================================

/// <summary>
/// Request DTO for completing a review with a verdict.
/// </summary>
public class PostCompleteReviewRequestDTO : IValidatableObject
{
    [Required]
    [Description("If the project is deemed enough to pass.")]
    public required bool Passed { get; init; }

    [Required]
    [Description("The various kinds of annotations made by the reviewer")]
    public IEnumerable<AnnotationData> Annotations { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Annotations.Count() > 256)
        {
            yield return new ValidationResult(
                "You can only submit at most 256 annotations in a single review",
                [nameof(Annotations)]
            );
        }

        if (Annotations.Count(a => a.Kind is AnnotationKind.Conclusion) > 1)
        {
            yield return new ValidationResult(
                "There can only be a single conclusion be made for completing a review",
                [nameof(Annotations)]
            );
        }
    }
}
