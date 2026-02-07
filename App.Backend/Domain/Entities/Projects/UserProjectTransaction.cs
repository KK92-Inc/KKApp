// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities.Projects;

/// <summary>
/// Tracks transactions related to user activities within a project.
/// </summary>
[Table("tbl_user_project_transactions")]
public class UserProjectTransaction : BaseEntity
{
    /// <summary>
    /// The user project associated with this transaction.
    /// </summary>
    [Column("user_project_id")]
    public Guid UserProjectId { get; set; }

    [ForeignKey(nameof(UserProjectId))]
    public virtual UserProject Creator { get; set; }

    /// <summary>
    /// The user associated with this transaction, if applicable.
    /// </summary>
    [Column("user_id")]
    public Guid? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    /// <summary>
    /// The type of activity performed in this transaction.
    /// </summary>
    [Column("type")]
    public UserProjectTransactionVariant Type { get; set; }
}
