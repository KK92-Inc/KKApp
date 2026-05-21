// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Reviews;

public class RubricVariantDO(RubricVariant variant)
{
    [Required]
    public ReviewKinds Kind { get; set; } = variant.Kind;

    [Required]
    public int RequiredCount { get; set; } = variant.Count;
}