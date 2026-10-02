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
    /// When the reviewer claimed this review, i.e. committed to doing it. Cleared again if the slot
    /// is released. Null while unclaimed, and for the pre-assigned self slot of a round (self reviews
    /// are on demand and never go stale).
    ///
    /// There is no scheduling: whoever claims a review has a fixed window to start it, and another
    /// fixed window to finish it once started. Deadlines are never stored, the cleanup job derives
    /// them from this and <see cref="StartedAt"/>.
    /// </summary>
    [Column("claimed_at")]
    public DateTimeOffset? ClaimedAt { get; set; }

    /// <summary>
    /// When the reviewer started working on the review. Set when the review moves to InProgress and
    /// cleared again if the slot is released. The cleanup job measures the finish window from this.
    /// </summary>
    [Column("started_at")]
    public DateTimeOffset? StartedAt { get; set; }

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