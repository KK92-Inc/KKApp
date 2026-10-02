// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Enums;

namespace App.Backend.Models.Requests.Reviews;

// ============================================================================

/// <summary>
/// Request DTO for self-service "giving" a review: the reviewer claims a review slot
/// for a user project and commits to a time, rather than waiting to be assigned one.
/// The review is created <see cref="ReviewState.Pending"/>, ready to be started (see
/// <c>POST /reviews/{id}/start</c>) and completed as usual once the reviewer sits down
/// to actually do it. The ref reviewed is always the project's default (master) branch.
/// </summary>
public class PostPushReviewRequestDTO : IValidatableObject
{
    [Required]
    [Description("The kind of review being given. Auto is currently not supported.")]
    public required ReviewKinds Kind { get; init; }

    [Required]
    [Description("When Kind is Peer, needs to be either today or tomorrow. Other kind of evaluations can leave this null.")]
    public DateTimeOffset? ScheduledAt { get; init; }

    [Description(@"
The user giving the review. Defaults to the caller; only staff may set this to another user.
For self reviews cannot be set.
")]
    public Guid? ReviewerId { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Kind is ReviewKinds.Self && ReviewerId is not null)
        {
            yield return new ValidationResult(
                "You cannot define the reviewer if doing a self evaluation",
                [nameof(ScheduledAt)]
            );
        }

        var now = DateTimeOffset.UtcNow;
        if (Kind is ReviewKinds.Peer)
        {
            if (ScheduledAt is null)
            {
                yield return new ValidationResult(
                    "Peer reviews need to be scheduled at a specific time.",
                    [nameof(ScheduledAt)]);
            }
            else if (ScheduledAt.Value.Date < now.Date || ScheduledAt.Value.Date > now.Date.AddDays(1))
            {
                yield return new ValidationResult(
                    "Peer reviews must be scheduled for today or tomorrow.",
                    [nameof(ScheduledAt)]);
            }
        }
    }
}