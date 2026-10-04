// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Requests.Kickoff;

/// <summary>
/// Request DTO for adding users to, or removing users from, a kickoff in bulk.
/// </summary>
public record KickoffUsersRequestDTO
{
    [Required, MinLength(1), MaxLength(256)]
    [Description("The ids of the users. Duplicates are ignored.")]
    public required Guid[] UserIds { get; init; }
}
