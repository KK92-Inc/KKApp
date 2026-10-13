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
public class KickoffDO
{
    public KickoffDO() { }

    public KickoffDO(Kickoff kickoff, int members)
    {
        Id = kickoff.Id;
        Name = kickoff.Name;
        Capacity = kickoff.Capacity;
        Members = members;
        StartsAt = kickoff.StartsAt;
        StartedAt = kickoff.StartedAt;
    }

    [Required, Description("The unique identifier of the kickoff.")]
    public Guid Id { get; set; }

    [Required, Description("The name of the kickoff.")]
    public string Name { get; set; } = null!;

    [Required, Description("The max amount of users that can join this kickoff.")]
    public int Capacity { get; set; }

    [Required, Description("How many users are in this kickoff right now.")]
    public int Members { get; set; }

    [Required, Description("The scheduled date on when this kickoff is invoked.")]
    public DateTimeOffset StartsAt { get; set; }

    [Description("When the kickoff actually started and its applicants were promoted. Null while it is still upcoming.")]
    public DateTimeOffset? StartedAt { get; set; }
}
