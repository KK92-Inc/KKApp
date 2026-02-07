// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Enums;

namespace App.Backend.Models.Requests.UserProjects;

// ============================================================================

/// <summary>
/// Request DTO for updating a user's project (partial update).
/// </summary>
public record PatchUserProjectRequestDTO
{
    /// <summary>
    /// Optional status update.
    /// </summary>
    public EntityObjectState? Status { get; init; }
}
