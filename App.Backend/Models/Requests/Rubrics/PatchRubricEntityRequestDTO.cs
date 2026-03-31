// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain;
using App.Backend.Domain.Enums;

namespace App.Backend.Models.Requests.Rubrics;

// ============================================================================

/// <summary>
/// Request DTO for updating a rubric entity (partial update).
/// </summary>
public record PatchRubricEntityRequestDTO
{
    /// <summary>
    /// Optional name update.
    /// </summary>
    [StringLength(256, MinimumLength = 1)]
    public string? Name { get; init; }

    /// <summary>
    /// Optional markdown content update.
    /// </summary>
    [StringLength(65536)]
    public string? Markdown { get; init; }

    /// <summary>
    /// Optional public status update.
    /// </summary>
    public bool? Public { get; init; }

    /// <summary>
    /// Optional enabled status update.
    /// </summary>
    public bool? Enabled { get; init; }

    /// <summary>
    /// Optional supported review kinds update.
    /// </summary>
    public ReviewKinds? SupportedReviewKinds { get; init; }

    /// <summary>
    /// Optional reviewer rules update.
    /// </summary>
    public ICollection<Rule>? ReviewerRules { get; init; }

    /// <summary>
    /// Optional reviewee rules update.
    /// </summary>
    public ICollection<Rule>? RevieweeRules { get; init; }
}
