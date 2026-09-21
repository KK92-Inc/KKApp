// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using App.Backend.Domain.Entities;

namespace App.Backend.Models.Responses.Entities;

// ============================================================================

/// <summary>
/// Response object describing a kickoff.
/// </summary>
public class KickoffDO(Kickoff kickoff)
{
    [Description("The unique identifier of the kickoff.")]
    public Guid Id { get; init; } = kickoff.Id;

    [Description("The name of the kickoff.")]
    public string Name { get; init; } = kickoff.Name;

    [Description("The max amount of users that can join this kickoff.")]
    public int Capacity { get; init; } = kickoff.Capacity;

    [Description("The scheduled date on when this kickoff is invoked.")]
    public DateTimeOffset StartsAt { get; init; } = kickoff.StartsAt;
}