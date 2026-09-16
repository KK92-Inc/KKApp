// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Requests;

public class SystemInitDTO : RequestDTO
{
    [Required, StringLength(255, MinimumLength = 1)]
    public required string Login { get; init; }

    [Required, StringLength(255, MinimumLength = 1)]
    public required string Firstname { get; init; }

    [Required, StringLength(255, MinimumLength = 1)]
    public required string Lastname { get; init; }

    [Required, EmailAddress, StringLength(255, MinimumLength = 1)]
    public required string Email { get; init; }
}
