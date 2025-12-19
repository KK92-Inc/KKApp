// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.UserProjects;

// ============================================================================

/// <summary>
/// Request DTO for adding a member to a user project.
/// </summary>
public record PostUserProjectMemberRequestDTO
{
    /// <summary>
    /// The user ID to add as a member.
    /// </summary>
    [Required]
    public required Guid UserId { get; init; }
}
