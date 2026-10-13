// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Models.Responses.Entities.Projects;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

/// <summary>
/// Data object representing a user's goal enrollment.
/// </summary>
public class WorkspaceDO : BaseEntityDO<Workspace>
{
    public WorkspaceDO() { }

    public WorkspaceDO(Workspace workspace) : base(workspace)
    {
        Owner = workspace.Owner;
        Ownership = workspace.Ownership;
    }

    [Required]
    public UserLightDO? Owner { get; set; }

    [Required]
    public EntityOwnership Ownership { get; set; }

    public static implicit operator WorkspaceDO?(Workspace? workspace) => workspace is null ? null : new(workspace);
}
