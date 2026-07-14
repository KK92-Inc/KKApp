// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace App.Backend.Models.Requests.Projects;

// ============================================================================

/// <summary>
/// Request DTO for updating a project (partial update).
/// </summary>
public record PatchProjectRequestDTO
{
    /// <summary>
    /// The name of the project.
    /// </summary>
    [StringLength(256, MinimumLength = 1)]
    [Description("The name of the project.")]
    public string? Name { get; init; }

    /// <summary>
    /// Optional description of the project.
    /// </summary>
    [StringLength(2048, MinimumLength = 1)]
    [Description("Optional description of the project.")]
    public string? Description { get; init; }

    /// <summary>
    /// Whether the project is active.
    /// </summary>
    [Description("Indicates whether the project is currently active.")]
    public bool? Active { get; init; }

    /// <summary>
    /// Whether the project is public.
    /// </summary>
    [Description("Indicates whether the project is currently public.")]
    public bool? Public { get; init; }

    /// <summary>
    /// The maximum number of members allowed in the project.
    /// </summary>
    [Range(1, 10)]
    [Description("The maximum number of members allowed in the project.")]
    public int? MaxMembers { get; init; } = 1;
}
