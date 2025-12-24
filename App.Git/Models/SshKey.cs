// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace App.Git.Models;

/// <summary>
/// Represents an SSH public key for git authentication.
/// </summary>
[Table("ssh_keys")]
public class SshKey : BaseEntity
{
    /// <summary>
    /// A user-friendly name for the key.
    /// </summary>
    [Column("title"), MaxLength(255), Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The full SSH public key (e.g., "ssh-ed25519 AAAA... user@host").
    /// </summary>
    [Column("public_key"), Required]
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// The SHA256 fingerprint of the key.
    /// </summary>
    [Column("fingerprint"), MaxLength(128), Required]
    public string Fingerprint { get; set; } = string.Empty;

    /// <summary>
    /// The key type (e.g., "ssh-ed25519", "ssh-rsa").
    /// </summary>
    [Column("key_type"), MaxLength(64)]
    public string KeyType { get; set; } = string.Empty;

    /// <summary>
    /// The last time this key was used.
    /// </summary>
    [Column("last_used_at")]
    public DateTimeOffset? LastUsedAt { get; set; }

    // Relations
    [JsonIgnore, Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}
