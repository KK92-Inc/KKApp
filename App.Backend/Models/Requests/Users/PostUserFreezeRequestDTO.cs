// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Backend.Models.Requests.Users;

// ============================================================================

public class PostUserFreezeRequestDTO : IValidatableObject
{
    [Required, StringLength(2048, MinimumLength = 4)]
    [Description("The reason for the freeze.")]
    public required string Reason { get; init; }

    [Required]
    [Description("When the freeze will start.")]
    public DateTimeOffset StartsAt { get; init; }

    [Required]
    [Description("When the freeze will end.")]
    public DateTimeOffset EndsAt { get; init; }

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
