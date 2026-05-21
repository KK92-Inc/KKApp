// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities.Reviews;

/// <summary>
/// Defines a variant of a rubric, i.e. a specific kind of review and how many reviews of this kind are required for the rubric.
/// This allows a single rubric to be used for multiple review kinds (e.g. self and peer) and to specify different requirements for each kind.
/// </summary>
[Table("tbl_rubric_variant")]
public class RubricVariant : BaseEntity
{
    [Column("rubric_id")]
    public Guid RubricId { get; set; }

    [Column("kind")]
    public ReviewKinds Kind { get; set; }

    /// <summary>
    /// How many reviews of this kind are required.
    /// Zero effectively disables this kind.
    /// </summary>
    [Column("count")]
    public int Count { get; set; }

    [ForeignKey(nameof(RubricId))]
    public virtual Rubric Rubric { get; set; } = null!;
}