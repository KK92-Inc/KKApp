// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Projects;

/// <summary>
/// Data object representing a project template/definition.
/// </summary>
public class ProjectDO(Project project) : BaseEntityDO<Project>(project)
{
    [Required]
    public string Name { get; set; } = project.Name;

    [Required]
    public string Description { get; set; } = project.Description;

    [Required]
    public string Slug { get; set; } = project.Slug;

    [Required]
    public bool Active { get; set; } = project.Active;

    [Required]
    public bool Public { get; set; } = project.Public;

    [Required]
    public bool Deprecated { get; set; } = project.Deprecated;

    // [Required]
    // public WorkspaceDO Workspace { get; set; } = project.;

    [Required]
    public WorkspaceDO Workspace { get; set; } = project.Workspace;

    public static implicit operator ProjectDO?(Project? project) => project is null ? null : new(project);
}
