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
/// Response object describing a trial period.
/// </summary>
public class TrialDO
{
    public TrialDO() { }

    public TrialDO(Trial trial, int participants)
    {
        Id = trial.Id;
        Name = trial.Name;
        CursusId = trial.CursusId;
        Capacity = trial.Capacity;
        Participants = participants;
        StartsAt = trial.StartsAt;
        EndsAt = trial.EndsAt;
        StartedAt = trial.StartedAt;
        EndedAt = trial.EndedAt;
    }

    [Required, Description("The unique identifier of the trial.")]
    public Guid Id { get; set; }

    [Required, Description("The name of the trial.")]
    public string Name { get; set; } = null!;

    [Required, Description("The cursus the participants work on during the trial.")]
    public Guid CursusId { get; set; }

    [Required, Description("The max amount of users that can take part in this trial.")]
    public int Capacity { get; set; }

    [Required, Description("How many users are in this trial, including the ones that quit.")]
    public int Participants { get; set; }

    [Required, Description("The scheduled date on when this trial begins.")]
    public DateTimeOffset StartsAt { get; set; }

    [Required, Description("The scheduled date on when this trial ends.")]
    public DateTimeOffset EndsAt { get; set; }

    [Description("When the trial actually began. Null while it is still upcoming.")]
    public DateTimeOffset? StartedAt { get; set; }

    [Description("When the trial actually ended. Null until then.")]
    public DateTimeOffset? EndedAt { get; set; }
}
