// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Values.Misc;
using App.Backend.Models.Validators;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.SshKeys;

// ============================================================================

/// <summary>
/// Request to add a new SSH public key.
/// </summary>
public class SystemInitDTO : RequestDTO
{
    [Required, StringLength(255, MinimumLength = 1)]
    public required string Login { get; init; }

    [Required, StringLength(255, MinimumLength = 6)]
    public required string Password { get; init; }

    [Required, EmailAddress, StringLength(255, MinimumLength = 1)]
    public required string Email { get; init; }
}
