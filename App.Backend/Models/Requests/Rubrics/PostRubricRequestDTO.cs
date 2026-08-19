// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Values.Misc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Rubrics;

// ============================================================================

/// <summary>
/// Request DTO for creating a new rubric entity.
/// </summary>
public record PostRubricRequestDTO
{
    [Required, StringLength(256, MinimumLength = 1)]
    public required string Name { get; init; }

    [Required, StringLength(2048, MinimumLength = 1)]
    public required string Description { get; init; }

    [Required]
    public bool Public { get; init; } = false;

    [Required]
    public bool Enabled { get; init; } = false;

    [Required]
    public Guid? ProjectId { get; init; } = null;

    [Required, MinLength(1)]
    public required IEnumerable<RubricVariantDTO> Variants { get; init; }

    [Required, MinLength(1)]
    public required IEnumerable<CommitFile> Files { get; init; }
}
