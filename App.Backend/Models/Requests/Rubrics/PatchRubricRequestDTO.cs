// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain;
using App.Backend.Domain.Enums;

namespace App.Backend.Models.Requests.Rubrics;

// ============================================================================

/// <summary>
/// Request DTO for updating a rubric entity (partial update).
/// </summary>
public class PatchRubricRequestDTO
{
    [StringLength(256, MinimumLength = 1)]
    public Optional<string> Name { get; init; }

    [StringLength(2048, MinimumLength = 1)]
    public Optional<string> Description { get; init; }

    public Optional<bool> Public { get; init; }

    public Optional<bool> Enabled { get; init; }
    
    public Optional<Guid?> ProjectId { get; init; }

    [Description("Indicates the variations of the rubric")]
    public required IEnumerable<RubricVariantDTO> Variants { get; init; }
}
