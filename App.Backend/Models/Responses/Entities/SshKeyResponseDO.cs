// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Users;

namespace App.Backend.Models.Responses.Entities;

/// <summary>
/// Response containing SSH key information.
/// </summary>
public class SshKeyResponseDO : ResponseDTO
{
    public SshKeyResponseDO() { }

    public SshKeyResponseDO(SshKey key)
    {
        Title = key.Title;
        Fingerprint = key.Fingerprint;
        KeyType = key.KeyType;
        CreatedAt = key.CreatedAt;
    }

    /// <summary>
    /// A user-friendly name for the key.
    /// </summary>
    [Required]
    public string Title { get; set; } = null!;

    /// <summary>
    /// The SHA256 fingerprint of the key (acts as the unique identifier).
    /// </summary>
    [Required]
    public string Fingerprint { get; set; } = null!;

    /// <summary>
    /// The key type (e.g., "ssh-ed25519", "ssh-rsa").
    /// </summary>
    [Required]
    public string KeyType { get; set; } = null!;

    /// <summary>
    /// When the key was added.
    /// </summary>
    [Required]
    public DateTimeOffset CreatedAt { get; set; }
}
