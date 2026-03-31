// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain;
using App.Backend.Domain.Enums;

namespace App.Backend.Models.Requests.Rubrics;

// ============================================================================

/// <summary>
/// Request DTO for creating a new rubric entity.
/// </summary>
public record PostRubricEntityRequestDTO
{
    /// <summary>
    /// The name of the rubric.
    /// </summary>
    [Required, StringLength(256, MinimumLength = 1)]
    public required string Name { get; init; }

    /// <summary>
    /// The markdown content describing the evaluation criteria.
    /// </summary>
    [StringLength(65536)]
    public string? Markdown { get; init; }

    /// <summary>
    /// Whether this rubric is publicly visible.
    /// </summary>
    public bool Public { get; init; } = false;

    /// <summary>
    /// Whether this rubric is currently enabled and can be used.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// The types of reviews this rubric supports.
    /// </summary>
    public ReviewKinds SupportedReviewKinds { get; init; } = ReviewKinds.All;

    /// <summary>
    /// Rules that determine who is eligible to be a reviewer.
    /// </summary>
    public ICollection<Rule>? ReviewerRules { get; init; }

    /// <summary>
    /// Rules that determine who is eligible to request a review (reviewee).
    /// </summary>
    public ICollection<Rule>? RevieweeRules { get; init; }
}
