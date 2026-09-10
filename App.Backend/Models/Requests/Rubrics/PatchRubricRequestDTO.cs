// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain;
using App.Backend.Domain.Enums;
using App.Backend.Models.Validators;

namespace App.Backend.Models.Requests.Rubrics;

// ============================================================================

/// <summary>
/// Request DTO for updating a rubric entity (partial update).
/// </summary>
public class PatchRubricRequestDTO
{
    [Required, StringLength(256, MinimumLength = 1)]
    public string Name { get; init; }

    [Required, StringLength(2048, MinimumLength = 1)]
    public string Description { get; init; }

    [Required]
    public bool Public { get; init; }

    [Required]
    public bool Enabled { get; init; }
    
    [Required]
    public Guid? ProjectId { get; init; }

    [Description("Indicates the variations of the rubric")]
    public IEnumerable<RubricVariantDTO> Variants { get; init; }
}
