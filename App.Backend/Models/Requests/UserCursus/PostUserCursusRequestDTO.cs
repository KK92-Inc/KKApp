// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.UserCursus;

// ============================================================================

/// <summary>
/// Request DTO for subscribing to a cursus.
/// </summary>
public record PostUserCursusRequestDTO
{
    /// <summary>
    /// The cursus ID to subscribe to.
    /// </summary>
    [Required]
    public required Guid CursusId { get; init; }
}
