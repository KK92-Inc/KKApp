// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities;

[Table("tbl_application")]
public class Application : BaseEntity
{
    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Column("name")]
    public required string Name { get; set; }

    [Column("client_id")]
    public required string ClientId { get; set; }

    [Column("description")]
    public required string Description { get; set; }

    [Column("enabled")]
    public required bool Enabled { get; set; }

    [Column("redirect_uris")]
    public ICollection<string> RedirectUris { get; set; } = [];

    [Column("scopes")]
    public ICollection<string> Scopes { get; set; } = [];

    [Column("workspace_id")]
    public Guid WorkspaceId { get; set; }

    [ForeignKey(nameof(WorkspaceId))]
    public virtual Workspace Workspace { get; set; }
}
