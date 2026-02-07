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
        Kind = ReviewVariant.Peer;
        State = ReviewState.Pending;

        ReviewerId = null;
        Reviewer = null;

        UserProjectId = Guid.Empty;
        UserProject = null!;

        RubricId = Guid.Empty;
        Rubric = null!;

        Comments = [];
    }

    // Columns //

    /// <summary>
    /// The type of review (Self, Peer, Async, Auto).
    /// </summary>
    [Column("kind")]
    public ReviewVariant Kind { get; set; }

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

    // Relations //

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
}
