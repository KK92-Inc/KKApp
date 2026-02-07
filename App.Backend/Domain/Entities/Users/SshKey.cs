// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Entities.Users;

/// <summary>
/// Represents an SSH public key associated with a user.
/// Users can have multiple SSH keys for git operations.
/// </summary>
[Table("tbl_ssh_key")]
public class SshKey : BaseTimestampEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SshKey"/> class.
    /// </summary>
    public SshKey()
    {
        Title = string.Empty;
        KeyType = string.Empty;
        KeyBlob = string.Empty;
        Fingerprint = string.Empty;
    }

    /// <summary>
    /// The SHA256 fingerprint of the key.
    /// This is computed automatically by the <see cref="SshKeyInterceptor"/>.
    /// </summary>
    [Column("fingerprint", Order = 0), Key]
    public string Fingerprint { get; set; }

    /// <summary>
    /// A user-friendly name for the key (e.g., "Work Laptop", "Personal Desktop").
    /// </summary>
    [Column("title"), MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The key type (e.g., "ssh-ed25519", "ssh-rsa", "ecdsa-sha2-nistp256").
    /// </summary>
    [Column("type"), MaxLength(64)]
    public string KeyType { get; set; } = string.Empty;

    /// <summary>
    /// The base64-encoded public key data.
    /// </summary>
    [Column("blob")]
    public string KeyBlob { get; set; } = string.Empty;

    //= Relations =//

    /// <summary>
    /// The user who owns this SSH key.
    /// </summary>
    [Column("user_id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}
