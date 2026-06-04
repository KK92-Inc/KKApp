// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Enums;

namespace App.Backend.Models.Requests.Reviews;

// ============================================================================

/// <summary>
/// Request DTO for updating a review (partial update).
/// </summary>
public record PatchReviewRequestDTO
{
    /// <summary>
    /// Optional feedback update.
    /// </summary>
    [StringLength(16384)]
    [Description("Feedback or comments on the review.")]
    public string? Feedback { get; init; }

    /// <summary>
    /// Optional status update.
    /// </summary>
    [Description("The current status of the review.")]
    public ReviewState? Status { get; init; }

    /// <summary>
    /// Optional final mark update.
    /// </summary>
    [Range(0, 100)]
    [Description("The final mark/score for the review (0-100).")]
    public int? FinalMark { get; init; }
}
