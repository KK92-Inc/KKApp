// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Projects;
using App.Backend.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities.Users;

/// <summary>
/// User projects are project "instances" / "sessions" for a project.
/// User's can partake in this session and complete it with the help of others.
/// </summary>
[Table("tbl_user_project")]
public class UserProject : BaseEntity
{
    public UserProject()
    {
        State = EntityObjectState.Inactive;

        Project = null!;
        ProjectId = Guid.Empty;

        GitInfo = null!;
        GitInfoId = Guid.Empty;

        Members = [];
        Transactions = [];
    }

    [Column("state")]
    public EntityObjectState State { get; set; }

    [Column("project_id")]
    public Guid ProjectId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public virtual Project Project { get; set; }

    [Column("git_info_id")]
    public Guid? GitInfoId { get; set; }

    [ForeignKey(nameof(GitInfoId))]
    public virtual Git? GitInfo { get; set; }

    /// <summary>
    /// Users partaking in this project session.
    /// </summary>
    public virtual ICollection<UserProjectMember> Members { get; set; }

    /// <summary>
    /// Transactions related to user activities within this project.
    /// </summary>
    public virtual ICollection<UserProjectTransaction> Transactions { get; set; }

}
