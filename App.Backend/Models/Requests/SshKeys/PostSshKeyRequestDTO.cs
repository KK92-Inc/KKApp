// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
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
    [Required, MaxLength(2048)]
    public required string PublicKey { get; set; }
}
