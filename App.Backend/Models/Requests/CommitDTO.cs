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
public class CommitDTO : RequestDTO
{

    [Required, StringLength(255, MinimumLength = 1)]
    [Description("The commit message")]
    public required string Message { get; init; }

    [Required, MinLength(1), MaxLength(10)]
    [Description("The files to commit")]
    public required IEnumerable<CommitFile> Files { get; init; }
}
