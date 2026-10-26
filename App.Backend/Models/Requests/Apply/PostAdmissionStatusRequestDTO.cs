// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Requests.Apply;

/// <summary>
/// Request DTO for looking up where an applicant is in the admissions.
/// </summary>
public class PostAdmissionStatusRequestDTO
{
    [Required, EmailAddress, StringLength(320, MinimumLength = 3)]
    [Description("The applicant's email address. Case doesn't matter.")]
    public required string Email { get; init; }
}
