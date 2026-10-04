// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

/// <summary>
/// Response object describing a kickoff.
/// </summary>
public class KickoffDO(Kickoff kickoff, int members)
{
    [Required, Description("The unique identifier of the kickoff.")]
    public Guid Id { get; init; } = kickoff.Id;

    [Required, Description("The name of the kickoff.")]
    public string Name { get; init; } = kickoff.Name;

    [Required, Description("The max amount of users that can join this kickoff.")]
    public int Capacity { get; init; } = kickoff.Capacity;

    [Required, Description("How many users are in this kickoff right now.")]
    public int Members { get; init; } = members;

    [Required, Description("The scheduled date on when this kickoff is invoked.")]
    public DateTimeOffset StartsAt { get; init; } = kickoff.StartsAt;

    [Description("When the kickoff actually started and its applicants were promoted. Null while it is still upcoming.")]
    public DateTimeOffset? StartedAt { get; init; } = kickoff.StartedAt;
}
