// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities;

/// <summary>
/// Represents a project entity within the application domain.
/// Projects are associated with workspaces and contain information about the project's name,
/// description, slug, and visibility settings.
///
/// The are templates for <see cref="Users.UserProject"/> instances.
/// </summary>
/// <remarks>
/// This class inherits from <see cref="BaseEntity"/>.
/// </remarks>
[Table("tbl_projects")]
[Index(nameof(Name)), Index(nameof(Slug))]
public class Project : BaseEntity
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
