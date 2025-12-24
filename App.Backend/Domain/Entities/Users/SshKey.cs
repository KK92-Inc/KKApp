// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Entities.Users;

/// <summary>
/// Represents an SSH public key associated with a user.
/// Users can have multiple SSH keys for git operations.
/// </summary>
[Table("tbl_ssh_key")]
public class SshKey : BaseEntity
{
    public SshKey()
    {
        Title = string.Empty;
        PublicKey = string.Empty;
        Fingerprint = string.Empty;
    }

    /// <summary>
    /// A user-friendly name for the key (e.g., "Work Laptop", "Personal Desktop").
    /// </summary>
    [Column("title"), MaxLength(255)]
    public string Title { get; set; }

    /// <summary>
    /// The full SSH public key (e.g., "ssh-ed25519 AAAA... user@host").
    /// </summary>
    [Column("public_key")]
    public string PublicKey { get; set; }

    /// <summary>
    /// The SHA256 fingerprint of the key for easy identification.
    /// </summary>
    [Column("fingerprint"), MaxLength(128)]
    public string Fingerprint { get; set; }

    /// <summary>
    /// The key type (e.g., "ssh-ed25519", "ssh-rsa", "ecdsa-sha2-nistp256").
    /// </summary>
    [Column("key_type"), MaxLength(64)]
    public string KeyType { get; set; } = string.Empty;

    /// <summary>
    /// Whether this key has been synced to the git server.
    /// </summary>
    [Column("synced_to_git_server")]
    public bool SyncedToGitServer { get; set; } = false;

    /// <summary>
    /// The last time this key was used for authentication.
    /// </summary>
    [Column("last_used_at")]
    public DateTimeOffset? LastUsedAt { get; set; }

    //= Relations =//

    /// <summary>
    /// The user who owns this SSH key.
    /// </summary>
    [JsonIgnore, Column("user_id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}
