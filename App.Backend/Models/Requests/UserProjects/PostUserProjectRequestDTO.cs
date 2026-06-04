// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.UserProjects;

// ============================================================================

/// <summary>
/// Request DTO for starting/subscribing to a project.
/// </summary>
public record PostUserProjectRequestDTO
{
    /// <summary>
    /// The project ID to subscribe to.
    /// </summary>
    [Required]
    [Description("The ID of the project to start/subscribe to.")]
    public required Guid ProjectId { get; init; }

    /// <summary>
    /// Optional cursus ID context for the project.
    /// </summary>
    [Description("Optional cursus ID context for the project.")]
    public Guid? CursusId { get; init; }
}
