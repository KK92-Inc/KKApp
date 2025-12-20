// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

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
    [StringLength(256)]
    public string? Kind { get; init; }

    /// <summary>
    /// Optional rubric data update as JSON.
    /// </summary>
    public string? Data { get; init; }
}
