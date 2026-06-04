// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
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
    [Description("The review ID this rubric response belongs to.")]
    public required Guid ReviewId { get; init; }

    /// <summary>
    /// The rubric kind/type.
    /// </summary>
    [Required, StringLength(256, MinimumLength = 1)]
    [Description("The rubric kind/type (e.g., 'technical', 'presentation').")]
    public required string Kind { get; init; }

    /// <summary>
    /// The rubric response data as JSON.
    /// </summary>
    [Required, StringLength(65536, MinimumLength = 1)]
    [Description("The rubric response data as JSON.")]
    public required string Data { get; init; }
}
