// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Enums;

namespace App.Backend.Models.Requests.Reviews;

// ============================================================================

/// <summary>
/// Request DTO for creating one or more review requests.
/// The user selects which kinds of reviews they want and a rubric.
/// One review entry is created per kind.
/// </summary>
public record PostReviewRequestDTO
{
    /// <summary>
    /// The user project ID being reviewed.
    /// </summary>
    [Required]
    public required Guid UserProjectId { get; init; }

    /// <summary>
    /// The rubric to use for evaluation.
    /// </summary>
    [Required]
    public required Guid RubricId { get; init; }

    /// <summary>
    /// The review kinds to request (e.g. Self, Peer, Async).
    /// One review is created per kind.
    /// </summary>
    [Required, MinLength(1)]
    public required ReviewVariant[] Kinds { get; init; }
}
