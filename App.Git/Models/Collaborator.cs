// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace App.Git.Models;

/// <summary>
/// Permission levels for collaborators.
/// </summary>
public enum CollaboratorPermission
{
    Read = 0,
    Write = 1,
    Admin = 2
}

/// <summary>
/// Represents a collaborator relationship between a user and repository.
/// </summary>
[Table("collaborators")]
public class Collaborator : BaseEntity
{
    /// <summary>
    /// The permission level granted to the collaborator.
    /// </summary>
    [Column("permission")]
    public CollaboratorPermission Permission { get; set; } = CollaboratorPermission.Read;

    // Relations
    [JsonIgnore, Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    [JsonIgnore, Column("repository_id")]
    public Guid RepositoryId { get; set; }

    [ForeignKey(nameof(RepositoryId))]
    public virtual Repository? Repository { get; set; }
}
