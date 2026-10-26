// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Requests.Apply;

/// <summary>
/// Request DTO for signing someone up for a check-in or an open day.
/// </summary>
public class PostApplyRegistrationRequestDTO
{
    [Required, EmailAddress, StringLength(320, MinimumLength = 3)]
    [Description("The applicant's email address. Case doesn't matter, it is stored lower-cased.")]
    public required string Email { get; init; }

    [StringLength(255)]
    [Description("The applicant's first name, if known.")]
    public string? FirstName { get; init; }

    [StringLength(255)]
    [Description("The applicant's last name, if known.")]
    public string? LastName { get; init; }
}
