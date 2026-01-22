// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Models.Responses.Entities.Cursus;
using App.Backend.Models.Responses.Entities.Projects;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

/// <summary>
/// Data object representing a user's goal enrollment.
/// </summary>
public class WorkspaceDO(Workspace workspace) : BaseEntityDO<Workspace>(workspace)
{
    [Required]
    public Guid Id { get; set; } = workspace.Id;

    [Required]
    public UserLightDO? Owner { get; set; } = workspace.Owner;

    [Required]
    public EntityOwnership Ownership { get; set; } = workspace.Ownership;

    public static implicit operator WorkspaceDO?(Workspace? workspace) => workspace is null ? null : new(workspace);
}
