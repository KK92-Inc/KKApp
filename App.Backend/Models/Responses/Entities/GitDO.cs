// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
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
public class GitDO(Git git) : BaseEntityDO<Git>(git)
{
    [Required]
    public string Name { get; set; } = git.Name;

    [Required]
    public string Namespace { get; set; } = git.Namespace;

    [Required]
    public string Url { get; set; } = git.Url;

    [Required]
    public EntityOwnership Ownership { get; set; } = git.Ownership;

    public static implicit operator GitDO?(Git? git) => git is null ? null : new(git);
}
