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
public class PostPushReviewRequestDTO
{
    /// <summary>
    /// The user project ID being reviewed.
    /// </summary>
    [Required]
    [Description("The user project ID being reviewed.")]
    public required Guid UserProjectId { get; init; }

    /// <summary>
    /// The kind of review being given. Only Peer and Async are supported here;
    /// Self reviews are auto-assigned on request, and Auto reviews aren't manual.
    /// </summary>
    [Required]
    [Description("The kind of review being given. Only Peer and Async are supported.")]
    public required ReviewKinds Kind { get; init; }

    /// <summary>
    /// When the reviewer commits to carrying out the review.
    /// For Async reviews, this must be now or within the next 2 hours.
    /// For Peer reviews, this must fall today or tomorrow.
    /// </summary>
    [Required]
    [Description("When the reviewer commits to doing the review. Async: now or within 2 hours. Peer: today or tomorrow.")]
    public required DateTimeOffset ScheduledAt { get; init; }

    /// <summary>
    /// The user giving the review. Defaults to the requesting user.
    /// Only staff may set this to someone other than themselves.
    /// </summary>
    [Description("The user giving the review. Defaults to the caller; only staff may set this to another user.")]
    public Guid? ReviewerId { get; init; }
}