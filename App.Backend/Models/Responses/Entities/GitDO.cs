// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

/// <summary>
/// Data object representing git repository information.
/// </summary>
public class GitDO : BaseEntityDO<GitInfo>
{
    public GitDO() { }

    public GitDO(GitInfo git) : base(git)
    {
        Name = git.Name;
        Owner = git.Owner;
        Ownership = git.Ownership;
    }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Owner { get; set; } = null!;

    [Required]
    public EntityOwnership Ownership { get; set; }

    public static implicit operator GitDO?(GitInfo? git) => git is null ? null : new(git);
}
