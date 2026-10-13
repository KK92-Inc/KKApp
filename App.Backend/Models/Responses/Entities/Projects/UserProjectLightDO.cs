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
public class UserProjectLightDO : BaseEntityDO<UserProject>
{
    public UserProjectLightDO() { }

    public UserProjectLightDO(UserProject userProject) : base(userProject)
    {
        State = userProject.State;
        Project = userProject.Project;
        GitInfo = userProject.GitInfo;
        UnlocksAt = userProject.UnlocksAt;
    }

    /// <summary>
    /// The current state of the object.
    /// </summary>
    [Required, Description("The current state of the object.")]
    public EntityObjectState State { get; set; }

    /// <summary>
    /// The project template this instance is based on.
    /// </summary>
    [Required, Description("The project template this instance is based on.")]
    public ProjectLightDO Project { get; set; } = null!;

    /// <summary>
    /// Git repository information for this project instance.
    /// </summary>
    [Required, Description("Git repository information for this project instance.")]
    public GitDO? GitInfo { get; set; }

    [Required, Description("Determines if and how long there is a cooldown on acting on this session again.")]
    public DateTimeOffset? UnlocksAt { get; set; }

    public static implicit operator UserProjectLightDO?(UserProject? userProject) =>
        userProject is null ? null : new(userProject);
}
