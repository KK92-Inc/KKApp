// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Projects;

public class ProjectLightDO(Project project) : BaseEntityDO<Project>(project)
{
    [Required, Description("The display name of the project.")]
    public string Name { get; set; } = project.Name;

    [Required, Description("A detailed description of the project's purpose or contents.")]
    public string Description { get; set; } = project.Description;

    [Required, Description("The URL-friendly slug identifier for the project.")]
    public string Slug { get; set; } = project.Slug;

    [Required, Description("Indicates whether the project is currently active.")]
    public bool Active { get; set; } = project.Active;

    [Required, Description("Indicates whether the project is publicly visible.")]
    public bool Public { get; set; } = project.Public;

    [Required, Description("Indicates whether the project has been deprecated.")]
    public bool Deprecated { get; set; } = project.Deprecated;

    /// <summary>
    /// Gets or sets a value indicating whether the project has been deprecated and should no longer be used.
    /// </summary>
    [Required, Description("Indicates whether the project has been deprecated.")]
    public int MaxMembers { get; set; } = project.MaxMembers;

    /// <summary>
    /// implicitly converts a <see cref="Project"/> entity to a <see cref="ProjectLightDO"/>.
    /// </summary>
    /// <param name="project">The project entity to convert.</param>
    /// <returns>A new instance of <see cref="ProjectLightDO"/> or null if the input is null.</returns>
    public static implicit operator ProjectLightDO?(Project? project) => project is null ? null : new(project);
}
