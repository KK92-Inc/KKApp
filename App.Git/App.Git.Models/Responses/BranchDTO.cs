// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Git.Models.Responses;

/// <summary>
/// Data object representing a member of a user project.
/// </summary>
public class BranchDTO(string Name, bool Head)
{
    /// <summary>
    /// Human friendly name of the branch.
    /// </summary>
    [Required]
    public string Name { get; init; } = Name;

    /// <summary>
    /// Is this branch the current head of the repository ?
    /// </summary>
    [Required]
    public bool Head { get; set; } = Head;
}
