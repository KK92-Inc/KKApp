// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Reviews;

// ============================================================================

/// <summary>
/// Request DTO for submitting a rubric response.
/// </summary>
public record PostRubricRequestDTO
{
    /// <summary>
    /// The review ID this rubric belongs to.
    /// </summary>
    [Required]
    public required Guid ReviewId { get; init; }

    /// <summary>
    /// The rubric kind/type.
    /// </summary>
    [Required, StringLength(256)]
    public required string Kind { get; init; }

    /// <summary>
    /// The rubric response data as JSON.
    /// </summary>
    [Required]
    public required string Data { get; init; }
}
