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
/// Represents a cursus (course) entity in the system.
/// Made up with a track that references goals.
/// </summary>
[Table("tbl_cursus")]
[Index(nameof(Name)), Index(nameof(Slug))]
public class Cursus : BaseEntity
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

    /// <summary>
    /// The track / path of the Cursus stored in the .graph format.
    /// </summary>
    [Column("track", TypeName = "jsonb")]
    public string? Track { get; set; }

    [Column("workspace_id")]
    public Guid WorkspaceId { get; set; }

    [ForeignKey(nameof(WorkspaceId))]
    public virtual Workspace Workspace { get; set; }
}
