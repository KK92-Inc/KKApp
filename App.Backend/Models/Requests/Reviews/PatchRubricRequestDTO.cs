// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Reviews;

// ============================================================================

/// <summary>
/// Request DTO for updating a rubric response.
/// </summary>
public record PatchRubricRequestDTO
{
    /// <summary>
    /// Optional kind update.
    /// </summary>
    [StringLength(256, MinimumLength = 1)]
    [Description("The rubric kind/type (e.g., 'technical', 'presentation').")]
    public string? Kind { get; init; }

    /// <summary>
    /// Optional rubric data update as JSON.
    /// </summary>
    [StringLength(65536, MinimumLength = 1)]
    [Description("The rubric response data as JSON.")]
    public string? Data { get; init; }
}
