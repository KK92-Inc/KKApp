// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Trials;

// ============================================================================

/// <summary>
/// Request DTO for updating a trial. Omitted (null) fields are left untouched.
/// </summary>
/// <remarks>
/// The start and end are checked against each other once merged with what is stored, not here.
/// </remarks>
public record PatchTrialRequestDTO
{
    [StringLength(255, MinimumLength = 1)]
    [Description("A new name for the trial.")]
    public string? Name { get; init; }

    [Description("A new cursus for the trial. Only possible before it starts.")]
    public Guid? CursusId { get; init; }

    [Range(1, int.MaxValue)]
    [Description("The new max amount of users that can take part in this trial.")]
    public int? Capacity { get; init; }

    [Description("The new start date. Only possible before it starts.")]
    public DateTimeOffset? StartsAt { get; init; }

    [Description("The new end date. Moving it earlier than now ends a running trial on the next run of the trial job.")]
    public DateTimeOffset? EndsAt { get; init; }
}
