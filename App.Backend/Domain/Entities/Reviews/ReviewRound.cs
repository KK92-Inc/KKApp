// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities.Reviews;

/// <summary>
/// One evaluation attempt of a user project. A round pins the rubric and the
/// commit that is being judged, and owns the review "slots" that must all
/// finish (with a passing verdict) for the project to be completed.
/// Reviews without a round are advisory feedback and never count.
/// </summary>
[Table("tbl_review_round")]
public class ReviewRound : BaseEntity
{
    public ReviewRound()
    {
        State = ReviewRoundState.Open;
        Ref = string.Empty;
        UserProject = null!;
        Rubric = null!;
        Reviews = [];
    }

    /// <summary>
    /// The user project being evaluated.
    /// </summary>
    [Column("user_project_id")]
    public Guid UserProjectId { get; set; }

    /// <summary>
    /// 1-based attempt number within the user project.
    /// </summary>
    [Column("attempt")]
    public int Attempt { get; set; }

    [Column("state")]
    public ReviewRoundState State { get; set; }

    /// <summary>
    /// The rubric this round was requested with.
    /// </summary>
    [Column("rubric_id")]
    public Guid RubricId { get; set; }

    /// <summary>
    /// The ref (branch name) that was submitted when the round was requested.
    /// </summary>
    [Column("ref")]
    public string Ref { get; set; }

    /// <summary>
    /// The exact commit every review of this round evaluates. Pinned at request time so the
    /// round stays reproducible after the branch moves on. Null for rounds created before this existed.
    /// </summary>
    [Column("sha")]
    public string Sha { get; set; }

    /// <summary>
    /// The team leader that requested the round.
    /// </summary>
    [Column("requested_by_id")]
    public Guid RequestedById { get; set; }

    /// <summary>
    /// When the round resolved (passed, failed or cancelled).
    /// </summary>
    [Column("closed_at")]
    public DateTimeOffset? ClosedAt { get; set; }

    // Relations //

    [ForeignKey(nameof(UserProjectId))]
    public virtual UserProject UserProject { get; set; }

    [ForeignKey(nameof(RubricId))]
    public virtual Rubric Rubric { get; set; }

    /// <summary>
    /// The review slots belonging to this round.
    /// </summary>
    public virtual ICollection<Review> Reviews { get; set; }
}
