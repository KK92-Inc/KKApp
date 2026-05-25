// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Reviews;

public class RubricVariantDO(RubricVariant variant)
{
    [Required, Description("The kind of review this variant represents.")]
    public ReviewKinds Kind { get; set; } = variant.Kind;

    [Required, Description("How many reviews of this kind are required.")]
    public int Requires { get; set; } = variant.Count;
}