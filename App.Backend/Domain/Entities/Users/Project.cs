// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Projects;
using App.Backend.Domain.Entities.Reviews;
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

        GitInfo = null;
        GitInfoId = null;

        Rubric = null;
        RubricId = null;

        Members = [];
        Transactions = [];
        Reviews = [];
    }

    /// <summary>
    /// The current state of this user project session.
    /// </summary>
    [Column("state")]
    public EntityObjectState State { get; set; }

    /// <summary>
    /// The project template this session is based on.
    /// </summary>
    [Column("project_id")]
    public Guid ProjectId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public virtual Project Project { get; set; }

    /// <summary>
    /// Optional Git repository for this user project's work.
    /// </summary>
    [Column("git_info_id")]
    public Guid? GitInfoId { get; set; }

    [ForeignKey(nameof(GitInfoId))]
    public virtual Git? GitInfo { get; set; }

    /// <summary>
    /// The rubric selected for evaluating this user project.
    /// If null, the default rubric from the project template is used.
    /// </summary>
    [Column("rubric_id")]
    public Guid? RubricId { get; set; }

    [ForeignKey(nameof(RubricId))]
    public virtual Rubric? Rubric { get; set; }

    /// <summary>
    /// Users partaking in this project session.
    /// </summary>
    public virtual ICollection<UserProjectMember> Members { get; set; }

    /// <summary>
    /// Transactions related to user activities within this project.
    /// </summary>
    public virtual ICollection<UserProjectTransaction> Transactions { get; set; }

    /// <summary>
    /// Reviews conducted for this user project.
    /// </summary>
    public virtual ICollection<Review> Reviews { get; set; }
}
