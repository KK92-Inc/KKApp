// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities;


/// <summary>
/// Represents a goal entity which is made up of many projects.
/// </summary>
/// <remarks>
/// To know which project belongs to which goal, see: <see cref="Relations.GoalProject"/>
/// </remarks>
[Table("tbl_goals")]
[Index(nameof(Name)), Index(nameof(Slug))]
public class Goal : BaseEntity
{
    [Column("name"), StringLength(255)]
    public required string Name { get; set; }

    [Column("description"), StringLength(2048)]
    public required string Description { get; set; }

    [Column("slug"), StringLength(255)]
    public required string Slug { get; set; }

    [Column("active")]
    public bool Active { get; set; }

    [Column("public")]
    public bool Public { get; set; }

    [Column("deprecated")]
    public bool Deprecated { get; set; }

    [Column("workspace_id")]
    public Guid WorkspaceId { get; set; }

    [ForeignKey(nameof(WorkspaceId))]
    public virtual Workspace Workspace { get; set; }
}
