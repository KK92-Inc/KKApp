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
[Index(nameof(KeycloakId), IsUnique = true)]
public class Application : BaseEntity
{
    [Column("kc_id", Order = 1)]
    public Guid KeycloakId { get; set; }

    [Column("name"), StringLength(255)]
    public required string Name { get; set; }

    [Column("client_id"), StringLength(255)]
    public required string ClientId { get; set; }

    [Column("description"), StringLength(2048)]
    public required string Description { get; set; }

    [Column("redirect_uris")]
    public ICollection<string> RedirectUris { get; set; } = [];

    [Column("workspace_id")]
    public Guid WorkspaceId { get; set; }

    [ForeignKey(nameof(WorkspaceId))]
    public virtual Workspace Workspace { get; set; }
}
