// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Git.Models;

/// <summary>
/// Represents a user in the git server.
/// </summary>
[Table("users")]
public class User : BaseEntity
{
    /// <summary>
    /// Unique username for the user.
    /// </summary>
    [Column("username"), MaxLength(255), Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the user.
    /// </summary>
    [Column("email"), MaxLength(255)]
    public string? Email { get; set; }

    /// <summary>
    /// Whether this user is an admin.
    /// </summary>
    [Column("is_admin")]
    public bool IsAdmin { get; set; } = false;

    /// <summary>
    /// External identity provider subject ID (for SSO/OAuth).
    /// </summary>
    [Column("external_id"), MaxLength(255)]
    public string? ExternalId { get; set; }

    // Navigation properties
    public virtual ICollection<SshKey> SshKeys { get; set; } = [];
    public virtual ICollection<Repository> OwnedRepositories { get; set; } = [];
    public virtual ICollection<Collaborator> Collaborations { get; set; } = [];
}
