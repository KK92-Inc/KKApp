// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Requests.Apply;

/// <summary>
/// Request DTO for creating a check-in or an open day.
/// </summary>
public class PostApplyEventRequestDTO : IValidatableObject
{
    [Required, StringLength(255, MinimumLength = 1)]
    [Description("A name for the session, e.g: Open Day::2026::November")]
    public required string Name { get; init; }

    [StringLength(2000)]
    [Description("What applicants can expect, e.g: what to bring.")]
    public string? Description { get; init; }

    [Required, StringLength(500, MinimumLength = 1)]
    [Description("Where it takes place, e.g: the address of the campus.")]
    public required string Location { get; init; }

    [Range(1, int.MaxValue)]
    [Description("The max amount of people that can sign up. Leave empty for no limit.")]
    public int? Capacity { get; init; }

    [Required, Description("When the session begins.")]
    public required DateTimeOffset StartsAt { get; init; }

    [Required, Description("The last moment someone can still sign up. Not later than the start.")]
    public required DateTimeOffset DeadlineAt { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (DeadlineAt > StartsAt)
        {
            yield return new ValidationResult(
                "The sign-up deadline can't be after the start.",
                [nameof(StartsAt), nameof(DeadlineAt)]);
        }
    }
}
