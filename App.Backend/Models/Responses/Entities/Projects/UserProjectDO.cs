// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Models.Responses.Entities.Reviews;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Projects;

/// <summary>
/// Data object representing a user's project instance/session.
/// </summary>
public class UserProjectDO(UserProject userProject) : BaseEntityDO<UserProject>(userProject)
{
    /// <summary>
    /// The current state of the object.
    /// </summary>
    [Required, Description("The current state of the object.")]
    public EntityObjectState State { get; set; } = userProject.State;

    /// <summary>
    /// The rubric selected for evaluating this project.
    /// </summary>
    [Required, Description("The rubric selected for evaluating this project.")]
    public Guid? RubricId { get; set; } = userProject.RubricId;

    /// <summary>
    /// The project template this instance is based on.
    /// </summary>
    [Required, Description("The project template this instance is based on.")]
    public ProjectDO Project { get; set; } = userProject.Project;

    /// <summary>
    /// Git repository information for this project instance.
    /// </summary>
    [Required, Description("Git repository information for this project instance.")]
    public GitDO? GitInfo { get; set; } = userProject.GitInfo;

    public static implicit operator UserProjectDO?(UserProject? userProject) =>
        userProject is null ? null : new(userProject);
}
