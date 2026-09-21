
// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Kickoff;

// ============================================================================

/// <summary>
/// Request DTO for creating a new kickoff.
/// </summary>
public record PostKickoffRequestDTO
{
    [Required, StringLength(255, MinimumLength = 1)]
    [Description("A name for the kickoff, e.g: Kickoff::2026::Summer")]
    public required string Name { get; init; }

    [Range(1, int.MaxValue)]
    [Description("The max amount of users that can join this kickoff.")]
    public required int Capacity { get; init; }

    [Required, Description("The scheduled date on when to invoke this kickoff.")]
    public required DateTimeOffset StartsAt { get; init; }
}