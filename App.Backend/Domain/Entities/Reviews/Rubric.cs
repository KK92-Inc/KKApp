// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using App.Backend.Domain;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Rules;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Domain.Entities.Reviews;

/// <summary>
/// A rubric is a standalone evaluation template that defines how a project
/// should be reviewed. It can be attached to projects or user projects.
/// </summary>
[Table("tbl_rubric")]
[Index(nameof(Name)), Index(nameof(Slug))]
public class Rubric : BaseEntity
{
    public Rubric()
    {
        Name = string.Empty;
        Slug = string.Empty;
        Markdown = string.Empty;
        Public = false;
        Enabled = false;

        CreatorId = Guid.Empty;
        Creator = null!;

        GitInfoId = null;
        GitInfo = null;

        SupportedReviewKinds = ReviewKinds.All;
        RevieweeRules = [];
        ReviewerRules = [];
        UserProjects = [];
        Reviews = [];
    }

    /// <summary>
    /// The name of the rubric.
    /// </summary>
    [Column("name")]
    public string Name { get; set; }

    /// <summary>
    /// URL-friendly unique identifier for the rubric.
    /// </summary>
    [Column("slug")]
    public string Slug { get; set; }

    /// <summary>
    /// The markdown content describing the evaluation criteria.
    /// </summary>
    [Column("markdown")]
    public string Markdown { get; set; }

    /// <summary>
    /// Whether this rubric is publicly visible.
    /// </summary>
    [Column("public")]
    public bool Public { get; set; }

    /// <summary>
    /// Whether this rubric is currently enabled and can be used.
    /// </summary>
    [Column("enabled")]
    public bool Enabled { get; set; }

    /// <summary>
    /// The types of reviews this rubric supports.
    /// </summary>
    [Column("supported_variants")]
    public ReviewKinds SupportedReviewKinds { get; set; }

    /// <summary>
    /// Rules that determine who is eligible to be a reviewer.
    /// Stored as JSON.
    /// </summary>
    [Column("reviewer_rules", TypeName = "jsonb")]
    public ICollection<Rule> ReviewerRules { get; set; }

    /// <summary>
    /// Rules that determine who is eligible to request a review (reviewee).
    /// Stored as JSON.
    /// </summary>
    [Column("reviewee_rules", TypeName = "jsonb")]
    public ICollection<Rule> RevieweeRules { get; set; }

    /// <summary>
    /// The user who created this rubric.
    /// </summary>
    [Column("creator_id")]
    public Guid CreatorId { get; set; }

    /// <summary>
    /// Optional Git repository containing additional rubric resources (tests, scripts).
    /// </summary>
    [Column("git_info_id")]
    public Guid? GitInfoId { get; set; }

    // Relations //

    [ForeignKey(nameof(CreatorId))]
    public virtual User Creator { get; set; }

    [ForeignKey(nameof(GitInfoId))]
    public virtual Git? GitInfo { get; set; }

    /// <summary>
    /// Reviews that have been conducted using this rubric.
    /// </summary>
    public virtual ICollection<Review> Reviews { get; set; }

    /// <summary>
    /// User projects that have selected this rubric for evaluation.
    /// </summary>
    public virtual ICollection<UserProject> UserProjects { get; set; }
}
