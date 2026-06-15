// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Values;
using App.Backend.Domain.Entities.Reviews;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Reviews;

public class AnnotationDO(Annotation review) : BaseEntityDO<Annotation>(review)
{
    [Required]
    public string FilePath { get; set; } = review.FilePath;

    [Required]
    public AnnotationData? Data { get; set; } = JsonSerializer.Deserialize<AnnotationData>(review.Data);

    [Required]
    public Guid ReviewId { get; set; } = review.ReviewId;

    [Required]
    public UserLightDO Author { get; set; } = new (review.Author);

    public static implicit operator AnnotationDO?(Annotation? annotation) =>
        annotation is null ? null : new(annotation);
}
