// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Enums;
using App.Backend.Domain.Entities.Users;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities.Reviews;

/// <summary>
/// A review entity representing a review made by a user on a project.
/// </summary>
[Table("tbl_review")]
public class Review : BaseEntity
{
    public Review()
    {
        Kind = ReviewKinds.Peer;
        State = ReviewState.Pending;

        ReviewerId = null;
        Reviewer = null;

        UserProjectId = Guid.Empty;
        UserProject = null!;

        RubricId = Guid.Empty;
        Rubric = null!;

        RoundId = null;
        Round = null;
        Passed = null;
        FinishedAt = null;

        Comments = [];
        Annotations = [];
    }

    // Columns //

    /// <summary>
    /// The type of review (Self, Peer, Async, Auto).
    /// </summary>
    [Column("kind")]
    public ReviewKinds Kind { get; set; }

    /// <summary>
    /// The current state of the review (Pending, InProgress, Finished).
    /// </summary>
    [Column("state")]
    public ReviewState State { get; set; }

    /// <summary>
    /// The user performing the review. Null for Auto reviews or unassigned reviews.
    /// </summary>
    [Column("reviewer_id")]
    public Guid? ReviewerId { get; set; }

    /// <summary>
    /// The user project that this review is evaluating.
    /// </summary>
    [Column("user_project_id")]
    public Guid UserProjectId { get; set; }

    /// <summary>
    /// The rubric used for this review's evaluation criteria.
    /// </summary>
    [Column("rubric_id")]
    public Guid RubricId { get; set; }

    /// <summary>
    /// The ref (branch name) that was targeted when the review was created, e.g. "master".
    /// This moves with every push, see <see cref="Sha"/> for what was actually reviewed.
    /// </summary>
    [Column("ref")]
    public string Ref { get; set; }

    /// <summary>
    /// The exact commit that was reviewed, resolved from <see cref="Ref"/> when the review
    /// was created. Null for reviews created before SHAs were recorded.
    /// </summary>
    [Column("sha")]
    public string? Sha { get; set; }

    /// <summary>
    /// When the reviewer has committed to carrying out the review.
    /// For Async reviews this is "now" or up to 2 hours out; for Peer reviews
    /// this is a time today or tomorrow. Null for reviews that don't go through
    /// the self-service "give a review" flow (e.g. Self, Auto, staff-assigned).
    /// </summary>
    [Column("scheduled_at")]
    public DateTimeOffset? ScheduledAt { get; set; }

    /// <summary>
    /// When a still-pending review should be considered stale and eligible for
    /// automatic cancellation. Currently only set for Async reviews.
    /// </summary>
    [Column("expires_at")]
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// The evaluation round this review is a slot of. Null means the review is
    /// advisory feedback: it can carry comments and annotations but it never
    /// counts towards (or against) completing the project.
    /// </summary>
    [Column("round_id")]
    public Guid? RoundId { get; set; }

    /// <summary>
    /// The reviewer's verdict: "Do you think this project is a pass?".
    /// Required to finish a review that belongs to a round, null otherwise.
    /// </summary>
    [Column("passed")]
    public bool? Passed { get; set; }

    /// <summary>
    /// When the review was finished.
    /// </summary>
    [Column("finished_at")]
    public DateTimeOffset? FinishedAt { get; set; }

    // Relations //

    [ForeignKey(nameof(RoundId))]
    public virtual ReviewRound? Round { get; set; }

    [ForeignKey(nameof(ReviewerId))]
    public virtual User? Reviewer { get; set; }

    [ForeignKey(nameof(UserProjectId))]
    public virtual UserProject UserProject { get; set; }

    [ForeignKey(nameof(RubricId))]
    public virtual Rubric Rubric { get; set; }

    /// <summary>
    /// Reviews are made up of multiple feedback entries/comments.
    /// </summary>
    public virtual Collection<Comment> Comments { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual Collection<Annotation> Annotations { get; set; }
}