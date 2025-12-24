// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.SshKeys;

// ============================================================================

/// <summary>
/// Request to add a new SSH public key.
/// </summary>
public class PostSshKeyRequestDTO : RequestDTO
{
    /// <summary>
    /// A user-friendly name for the key (e.g., "Work Laptop", "Personal Desktop").
    /// </summary>
    [Required, MaxLength(255)]
    public required string Title { get; set; }

    /// <summary>
    /// The full SSH public key (e.g., "ssh-ed25519 AAAA... user@host").
    /// </summary>
    [Required]
    public required string PublicKey { get; set; }
}

/// <summary>
/// Request to update an SSH key's title.
/// </summary>
public class PatchSshKeyRequestDTO : RequestDTO
{
    /// <summary>
    /// A new user-friendly name for the key.
    /// </summary>
    [MaxLength(255)]
    public string? Title { get; set; }
}

/// <summary>
/// Response containing SSH key information.
/// </summary>
public class SshKeyResponseDTO : ResponseDTO
{
    /// <summary>
    /// The unique identifier of the SSH key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// A user-friendly name for the key.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The SHA256 fingerprint of the key.
    /// </summary>
    public string Fingerprint { get; set; } = string.Empty;

    /// <summary>
    /// The key type (e.g., "ssh-ed25519", "ssh-rsa").
    /// </summary>
    public string KeyType { get; set; } = string.Empty;

    /// <summary>
    /// When the key was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// When the key was last used for authentication.
    /// </summary>
    public DateTimeOffset? LastUsedAt { get; set; }
}
