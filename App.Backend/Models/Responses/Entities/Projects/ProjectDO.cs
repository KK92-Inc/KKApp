// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Projects;

/// <summary>
/// Represents a data object for a project entity, containing details such as its identification, visibility status, and associated git information.
/// </summary>
public class ProjectDO(Project project)
{
    /// <summary>
    /// Gets or sets the display name of the project.
    /// </summary>
    [Required, Description("The display name of the project.")]
    public string Name { get; set; } = project.Name;

    /// <summary>
    /// Gets or sets a detailed description of the project's purpose or contents.
    /// </summary>
    [Required, Description("A detailed description of the project's purpose or contents.")]
    public string Description { get; set; } = project.Description;

    /// <summary>
    /// Gets or sets the URL-friendly slug identifier for the project.
    /// </summary>
    [Required, Description("The URL-friendly slug identifier for the project.")]
    public string Slug { get; set; } = project.Slug;

    /// <summary>
    /// Gets or sets a value indicating whether the project is currently active.
    /// </summary>
    [Required, Description("Indicates whether the project is currently active.")]
    public bool Active { get; set; } = project.Active;

    /// <summary>
    /// Gets or sets a value indicating whether the project is publicly visible.
    /// </summary>
    [Required, Description("Indicates whether the project is publicly visible.")]
    public bool Public { get; set; } = project.Public;

    /// <summary>
    /// Gets or sets a value indicating whether the project has been deprecated and should no longer be used.
    /// </summary>
    [Required, Description("Indicates whether the project has been deprecated.")]
    public bool Deprecated { get; set; } = project.Deprecated;

    /// <summary>
    /// Gets or sets the associated Git repository information for this project, if applicable.
    /// </summary>
    [Required, Description("The associated Git repository information for this project.")]
    public GitDO? GitInfo { get; set; } = project.Git;

    /// <summary>
    /// Gets or sets the workspace to which this project belongs.
    /// </summary>
    [Required, Description("The workspace to which this project belongs.")]
    public WorkspaceDO Workspace { get; set; } = project.Workspace;

    /// <summary>
    /// implicitly converts a <see cref="Project"/> entity to a <see cref="ProjectDO"/>.
    /// </summary>
    /// <param name="project">The project entity to convert.</param>
    /// <returns>A new instance of <see cref="ProjectDO"/> or null if the input is null.</returns>
    public static implicit operator ProjectDO?(Project? project) => project is null ? null : new(project);
}
