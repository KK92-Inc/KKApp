// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Projects;

public class ProjectDO(Project project) : ProjectLightDO(project)
{
    [Required, Description("The associated Git repository information for this project.")]
    public GitDO GitInfo { get; set; } = project.Git;

    [Required, Description("The workspace to which this project belongs.")]
    public WorkspaceDO Workspace { get; set; } = project.Workspace;

    /// <summary>
    /// implicitly converts a <see cref="Project"/> entity to a <see cref="ProjectDO"/>.
    /// </summary>
    /// <param name="project">The project entity to convert.</param>
    /// <returns>A new instance of <see cref="ProjectDO"/> or null if the input is null.</returns>
    public static implicit operator ProjectDO?(Project? project) => project is null ? null : new(project);
}
