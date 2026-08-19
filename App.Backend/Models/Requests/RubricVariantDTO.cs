// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests;

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
