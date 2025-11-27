// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

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
        State = EntityState.Inactive;

        Project = null!;
        ProjectId = Guid.Empty;

        GitInfo = null!;
        GitInfoId = Guid.Empty;

        Users = null!;
    }

    [Column("state")]
    public EntityState State { get; set; }

    [Column("project_id")]
    public Guid ProjectId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public virtual Project Project { get; set; }

    [Column("git_info_id")]
    public Guid? GitInfoId { get; set; }

    [ForeignKey(nameof(GitInfoId))]
    public virtual Git? GitInfo { get; set; }

    public virtual ICollection<User> Users { get; set; }

}
