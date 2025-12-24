// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace App.Git.Models;

/// <summary>
/// Repository visibility levels.
/// </summary>
public enum RepositoryVisibility
{
    Private = 0,
    Internal = 1, // Visible to authenticated users
    Public = 2
}

/// <summary>
/// Represents a git repository.
/// </summary>
[Table("repositories")]
public class Repository : BaseEntity
{
    /// <summary>
    /// The repository name (unique per owner).
    /// </summary>
    [Column("name"), MaxLength(255), Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the repository.
    /// </summary>
    [Column("description"), MaxLength(1024)]
    public string? Description { get; set; }

    /// <summary>
    /// The path on disk where the bare repository is stored.
    /// </summary>
    [Column("path"), MaxLength(1024), Required]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The default branch name.
    /// </summary>
    [Column("default_branch"), MaxLength(255)]
    public string DefaultBranch { get; set; } = "main";

    /// <summary>
    /// Repository visibility.
    /// </summary>
    [Column("visibility")]
    public RepositoryVisibility Visibility { get; set; } = RepositoryVisibility.Private;

    /// <summary>
    /// Whether the repository is archived (read-only).
    /// </summary>
    [Column("is_archived")]
    public bool IsArchived { get; set; } = false;

    // Relations
    [JsonIgnore, Column("owner_id")]
    public Guid OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public virtual User? Owner { get; set; }

    public virtual ICollection<Collaborator> Collaborators { get; set; } = [];
}
