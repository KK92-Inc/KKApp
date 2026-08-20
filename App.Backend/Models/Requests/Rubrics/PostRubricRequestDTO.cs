// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Values.Misc;
using App.Backend.Models.Validators;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Rubrics;

// ============================================================================

/// <summary>
/// Request DTO for creating a new rubric entity.
/// </summary>
public class PostRubricRequestDTO
{
    [Required, StringLength(256, MinimumLength = 1)]
    public required string Name { get; init; }

    [Required]
    public bool Public { get; init; }

    [Required]
    public bool Enabled { get; init; }

    public required Guid? ProjectId { get; init; }

    [Required, MinLength(1, ErrorMessage = "Requires at least 1 variant to be defined.")]
    public required IEnumerable<RubricVariantDTO> Variants { get; init; }

    [RequiresFile("README.md", ErrorMessage = "You need to provide a file 'README.md' which is used as the rubric.")]
    [Required, MinLength(1, ErrorMessage = "Requires at least 1 file to be defined.")]
    public required IEnumerable<CommitFile> Files { get; init; }
}
