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
/// for a user project, rather than waiting to be assigned one.
/// The review is created <see cref="ReviewState.Pending"/>, ready to be started (see
/// <c>POST /reviews/{id}/start</c>) and completed as usual. There is no scheduling, the
/// reviewer has a fixed window to start it before the slot is released. The ref reviewed is always the project's default (master) branch.
/// </summary>
public class PostPushReviewRequestDTO : IValidatableObject
{
    [Required]
    [Description("The kind of review being given. Auto is currently not supported and Peer slots are claimed by assignment.")]
    public required ReviewKinds Kind { get; init; }

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
                [nameof(ReviewerId)]
            );
        }
    }
}