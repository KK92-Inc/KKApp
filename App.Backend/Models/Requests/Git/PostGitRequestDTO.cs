// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Git;

// ============================================================================

/// <summary>
/// Request DTO for creating a new git configuration.
/// </summary>
public record PostGitRequestDTO
{
    /// <summary>
    /// The remote URL for the git repository.
    /// </summary>
    [Required, Url]
    public required string Remote { get; init; }

    /// <summary>
    /// The branch name to use.
    /// </summary>
    [Required, StringLength(256, MinimumLength = 1)]
    public required string Branch { get; init; }

    /// <summary>
    /// The specific commit SHA to reference.
    /// </summary>
    [StringLength(40, MinimumLength = 40)]
    [RegularExpression(@"^[a-fA-F0-9]{40}$", ErrorMessage = "Commit must be a valid SHA-1 hash")]
    public string? Commit { get; init; }
}
