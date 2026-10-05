// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Trials;

// ============================================================================

/// <summary>
/// Request DTO for creating a new trial period.
/// </summary>
public record PostTrialRequestDTO : IValidatableObject
{
    [Required, StringLength(255, MinimumLength = 1)]
    [Description("A name for the trial, e.g: Piscine::2026::August")]
    public required string Name { get; init; }

    [Required, Description("The cursus the participants work on during the trial.")]
    public required Guid CursusId { get; init; }

    [Range(1, int.MaxValue)]
    [Description("The max amount of users that can take part in this trial.")]
    public required int Capacity { get; init; }

    [Required, Description("When the trial begins. Participants are activated then.")]
    public required DateTimeOffset StartsAt { get; init; }

    [Required, Description("When the trial ends. Participants are wrapped up then.")]
    public required DateTimeOffset EndsAt { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (StartsAt >= EndsAt)
        {
            yield return new ValidationResult(
                "The start date must be earlier than the end date.",
                [nameof(StartsAt), nameof(EndsAt)]);
        }
    }
}
