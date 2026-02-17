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
public class SshKeyResponseDO(SshKey key) : ResponseDTO
{
    /// <summary>
    /// A user-friendly name for the key.
    /// </summary>
    [Required]
    public string Title { get; set; } = key.Title;

    /// <summary>
    /// The SHA256 fingerprint of the key (acts as the unique identifier).
    /// </summary>
    [Required]
    public string Fingerprint { get; set; } = key.Fingerprint;

    /// <summary>
    /// The key type (e.g., "ssh-ed25519", "ssh-rsa").
    /// </summary>
    [Required]
    public string KeyType { get; set; } = key.KeyType;

    /// <summary>
    /// When the key was added.
    /// </summary>
    [Required]
    public DateTimeOffset CreatedAt { get; set; } = key.CreatedAt;
}
