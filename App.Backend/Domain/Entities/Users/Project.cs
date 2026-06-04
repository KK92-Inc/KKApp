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
        UnlocksAt = null;

        Project = null!;
        ProjectId = Guid.Empty;

        Reviews = [];
        Transactions = [];
    }

    /// <summary>
    /// The current state of this user project session.
    /// </summary>
    [Column("state")]
    public EntityObjectState State { get; set; }

    /// <summary>
    /// If set, locks modifications on the entity until the specified time.
    /// E.g: Used for cooldowns after unsubscribing.
    /// </summary>
    [Column("unlocks_at")]
    public DateTimeOffset? UnlocksAt { get; set; }

    /// <summary>
    /// The project template this session is based on.
    /// </summary>
    [Column("project_id")]
    public Guid ProjectId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public virtual Project Project { get; set; }

    [Column("git_info_id")]
    public Guid GitInfoId { get; set; }

    [ForeignKey(nameof(GitInfoId))]
    public virtual Git GitInfo { get; set; }

    /// <summary>
    /// Transactions related to user activities within this project.
    /// </summary>
    public virtual ICollection<UserProjectTransaction> Transactions { get; set; }

    /// <summary>
    /// Reviews conducted for this user project.
    /// </summary>
    public virtual ICollection<Review> Reviews { get; set; }
}
