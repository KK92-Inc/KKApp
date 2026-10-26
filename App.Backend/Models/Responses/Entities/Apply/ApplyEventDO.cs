// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Apply;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Apply;

/// <summary>
/// Response object describing a check-in or an open day.
/// </summary>
public class ApplyEventDO
{
    public ApplyEventDO() { }

    public ApplyEventDO(ApplyEvent session, int registered, int attended)
    {
        Id = session.Id;
        Name = session.Name;
        Description = session.Description;
        Location = session.Location;
        Capacity = session.Capacity;
        Registered = registered;
        Attended = attended;
        StartsAt = session.StartsAt;
        DeadlineAt = session.DeadlineAt;
    }

    [Required, Description("The unique identifier of the session.")]
    public Guid Id { get; set; }

    [Required, Description("The name of the session.")]
    public string Name { get; set; } = null!;

    [Required, Description("What applicants can expect. Empty if there is nothing to say.")]
    public string Description { get; set; } = null!;

    [Required, Description("Where it takes place.")]
    public string Location { get; set; } = null!;

    [Description("The max amount of people that can sign up. Null means there is no limit.")]
    public int? Capacity { get; set; }

    [Required, Description("How many people are signed up.")]
    public int Registered { get; set; }

    [Required, Description("How many of those were verified as present by a staff member.")]
    public int Attended { get; set; }

    [Required, Description("When the session begins.")]
    public DateTimeOffset StartsAt { get; set; }

    [Required, Description("The last moment someone can still sign up.")]
    public DateTimeOffset DeadlineAt { get; set; }
}
