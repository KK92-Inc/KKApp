// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Enums;

namespace App.Backend.Models.Requests.Reviews;

// ============================================================================

/// <summary>
/// Request DTO for creating a new review.
/// </summary>
public record PostReviewRequestDTO
{
    /// <summary>
    /// The user project ID being reviewed.
    /// </summary>
    [Required]
    public required Guid UserProjectId { get; init; }

    /// <summary>
    /// The ID of the user performing the review.
    /// </summary>
    [Required]
    public required Guid ReviewerId { get; init; }

    /// <summary>
    /// Optional feedback/comments on the review.
    /// </summary>
    [StringLength(16384)]
    public string? Feedback { get; init; }

    /// <summary>
    /// The status of the review.
    /// </summary>
    public ReviewState Status { get; init; } = ReviewState.Pending;
}
