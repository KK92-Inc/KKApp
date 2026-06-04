// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Models.Validators;
using System.ComponentModel;
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
    [Required, StringLength(255, MinimumLength = 1)]
    [Description("A user-friendly name for the SSH key (e.g., 'Work Laptop').")]
    public required string Title { get; set; }

    /// <summary>
    /// The full SSH public key (e.g., "ssh-ed25519 AAAA... user@host").
    /// </summary>
    [Required, StringLength(2048, MinimumLength = 1), PublicKey]
    public required string PublicKey { get; set; }
}
