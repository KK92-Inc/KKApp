// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Git;

// ============================================================================

/// <summary>
/// Request DTO for updating a git configuration (partial update).
/// </summary>
public record PatchGitRequestDTO
{
    /// <summary>
    /// Optional remote URL update.
    /// </summary>
    [Url]
    [Description("The remote URL for the git repository.")]
    public string? Remote { get; init; }

    /// <summary>
    /// Optional branch name update.
    /// </summary>
    [StringLength(256, MinimumLength = 1)]
    [Description("The branch name to use (e.g., 'main', 'develop').")]
    public string? Branch { get; init; }

    /// <summary>
    /// Optional commit SHA update.
    /// </summary>
    [StringLength(40, MinimumLength = 40)]
    [RegularExpression(@"^[a-fA-F0-9]{40}$", ErrorMessage = "Commit must be a valid SHA-1 hash")]
    [Description("The specific commit SHA-1 hash to reference.")]
    public string? Commit { get; init; }
}
