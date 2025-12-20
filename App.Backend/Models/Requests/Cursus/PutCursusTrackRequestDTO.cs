// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Cursus;

// ============================================================================

/// <summary>
/// Request DTO for replacing the track definition of a cursus.
/// </summary>
public record PutCursusTrackRequestDTO
{
    /// <summary>
    /// The track definition as JSON. This will replace the entire track.
    /// </summary>
    [Required]
    public required string Track { get; init; }
}
