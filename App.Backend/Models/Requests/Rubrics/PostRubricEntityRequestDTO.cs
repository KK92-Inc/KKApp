// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Rubrics;

// ============================================================================

/// <summary>
/// Request DTO for creating a new rubric entity.
/// </summary>
public record PostRubricEntityRequestDTO
{
    [Required, StringLength(256, MinimumLength = 1)]
    public required string Name { get; init; }

    [StringLength(65536)]
    public string? Markdown { get; init; } = null;

    public bool Public { get; init; } = false;

    public Guid? ProjectId { get; init; } = null;

    public bool Enabled { get; init; } = false;
}

/// <summary>
/// Defines a single review kind and how many are required.
/// </summary>
public record RubricVariantDTO
{
    [Required]
    public required ReviewKinds Kind { get; init; }

    [Required, Range(0, 100)]
    public required int Required { get; init; }
}

/// <summary>
/// Replaces the entire variants configuration for a rubric.
/// Kinds omitted are treated as disabled (count = 0).
/// </summary>
public record PutRubricVariantsRequestDTO
{
    [Required, MinLength(1)]
    public required IEnumerable<RubricVariantDTO> Variants { get; init; }
}