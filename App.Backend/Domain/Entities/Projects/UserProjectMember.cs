// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities.Projects;

/// <summary>
/// Tracks members of a user project and their roles.
/// </summary>
[Table("tbl_user_project_members")]
public class UserProjectMember : BaseEntity
{

    [Column("user_project_id")]
    public Guid UserProjectId { get; set; }

    [ForeignKey(nameof(UserProjectId))]
    public virtual UserProject Creator { get; set; }


    [Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }

    [Column("role")]
    public UserProjectRole Role { get; set; }

    [Column("left_at")]
    public DateTimeOffset? LeftAt { get; set; }

    // TODO: Add contribution score tracking later
    // Get's calculated based on commits (and their size), needs more time
    // [Column("score")]
    // public decimal ContributionScore { get; set; }
}
