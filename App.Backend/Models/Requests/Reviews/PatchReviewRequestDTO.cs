// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

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
    public string? Feedback { get; init; }

    /// <summary>
    /// Optional status update.
    /// </summary>
    public ReviewState? Status { get; init; }

    /// <summary>
    /// Optional final mark update.
    /// </summary>
    [Range(0, 100)]
    public int? FinalMark { get; init; }
}
