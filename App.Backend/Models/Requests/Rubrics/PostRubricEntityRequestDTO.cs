// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Rubrics;

// ============================================================================

public class RubricInitialFilesRequestDTO
{
    [Required, StringLength(256, MinimumLength = 1)]
    [Description("The path of the file")]
    public required string Path { get; init; }

    [Required, StringLength(256, MinimumLength = 1)]
    [Description("The content of the file")]
    public required string Content { get; init; }
}

/// <summary>
/// Request DTO for creating a new rubric entity.
/// </summary>
public record PostRubricRequestDTO
{
    [Required, StringLength(256, MinimumLength = 1)]
    [Description("The name of the rubric.")]
    public required string Name { get; init; }

    [StringLength(65536)]
    [Description("Optional markdown documentation for the rubric.")]
    public string? Markdown { get; init; } = null;

    [Description("Indicates whether the rubric is publicly visible.")]
    public bool Public { get; init; } = false;

    [Description("Optional project ID this rubric is associated with.")]
    public Guid? ProjectId { get; init; } = null;

    [Description("Indicates whether the rubric is enabled.")]
    public bool Enabled { get; init; } = false;

    [Required, MinLength(1)]
    [Description("The list of review variant requirements for this rubric.")]
    public required IEnumerable<RubricVariantDTO> Variants { get; init; }
}

/// <summary>
/// Defines a single review kind and how many are required.
/// </summary>
public record RubricVariantDTO
{
    [Required]
    [Description("The review kind this variant applies to.")]
    public required ReviewKinds Kind { get; init; }

    [Required, Range(0, 100)]
    [Description("The number of reviews of this kind required (0-100).")]
    public required int Required { get; init; }
}

/// <summary>
/// Replaces the entire variants configuration for a rubric.
/// Kinds omitted are treated as disabled (count = 0).
/// </summary>
public record PutRubricVariantsRequestDTO
{
    [Required, MinLength(1)]
    [Description("The list of review variant requirements for this rubric.")]
    public required IEnumerable<RubricVariantDTO> Variants { get; init; }
}