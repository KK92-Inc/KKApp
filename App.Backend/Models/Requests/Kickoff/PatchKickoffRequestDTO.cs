
// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Kickoff;

// ============================================================================

/// <summary>
/// Request DTO for updating a kickoff. Omitted (null) fields are left untouched.
/// </summary>
public record PatchKickoffRequestDTO
{
    [StringLength(255, MinimumLength = 1)]
    [Description("A new name for the kickoff.")]
    public string? Name { get; init; }

    [Range(1, int.MaxValue)]
    [Description("The new max amount of users that can join this kickoff.")]
    public int? Capacity { get; init; }

    [Description("The new scheduled date on when to invoke this kickoff.")]
    public DateTimeOffset? StartsAt { get; init; }
}