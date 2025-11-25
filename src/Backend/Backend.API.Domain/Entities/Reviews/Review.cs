// ============================================================================
// Copyright (c) 2025 - W2Inc.
// See README.md in the project root for license information.
// ============================================================================

using Backend.API.Domain.Enums;
using Backend.API.Domain.Entities.Users;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace Backend.API.Domain.Entities.Reviews;

/// <summary>
/// A review entity representing a review made by a user on a project.
/// </summary>
[Table("tbl_review")]
public class Review : BaseEntity
{
    // Columns //

    /// <summary>
    /// The type of review
    /// </summary>
    [Column("kind")]
    public ReviewVariant Kind { get; set; }

    /// <summary>
    /// The current state of the review
    /// </summary>
    [Column("state")]
    public ReviewState State { get; set; }

    /// <summary>
    /// The user doing the review.
    /// </summary>
    [Column("reviewer_id")]
    public Guid? ReviewerId { get; set; }

    /// <summary>
    /// The user project that this review is targeting.
    /// </summary>
    [Column("user_project_id")]
    public Guid UserProjectId { get; set; }

    // Relations //

    [ForeignKey(nameof(ReviewerId))]
    public virtual User? Reviewer { get; set; }

    // [ForeignKey(nameof(UserProjectId))]
    // public virtual UserProject UserProject { get; set; }

    /// <summary>
    /// Reviews are made up of multiple feedback entries.
    /// E.g: 
    /// </summary>
    public virtual Collection<Feedback> Feedbacks { get; set; }
}
