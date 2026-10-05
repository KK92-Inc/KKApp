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
public class TrialDO(Trial trial, int participants)
{
    [Required, Description("The unique identifier of the trial.")]
    public Guid Id { get; init; } = trial.Id;

    [Required, Description("The name of the trial.")]
    public string Name { get; init; } = trial.Name;

    [Required, Description("The cursus the participants work on during the trial.")]
    public Guid CursusId { get; init; } = trial.CursusId;

    [Required, Description("The max amount of users that can take part in this trial.")]
    public int Capacity { get; init; } = trial.Capacity;

    [Required, Description("How many users are in this trial, including the ones that quit.")]
    public int Participants { get; init; } = participants;

    [Required, Description("The scheduled date on when this trial begins.")]
    public DateTimeOffset StartsAt { get; init; } = trial.StartsAt;

    [Required, Description("The scheduled date on when this trial ends.")]
    public DateTimeOffset EndsAt { get; init; } = trial.EndsAt;

    [Description("When the trial actually began. Null while it is still upcoming.")]
    public DateTimeOffset? StartedAt { get; init; } = trial.StartedAt;

    [Description("When the trial actually ended. Null until then.")]
    public DateTimeOffset? EndedAt { get; init; } = trial.EndedAt;
}
