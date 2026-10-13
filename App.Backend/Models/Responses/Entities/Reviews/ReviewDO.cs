// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Reviews;

/// <summary>
/// The project session a review belongs to, trimmed to what a review view needs.
/// </summary>
public class ReviewProjectDO
{
    public ReviewProjectDO() { }

    public ReviewProjectDO(UserProject userProject)
    {
        Id = userProject.Id;
        State = userProject.State;
        ProjectId = userProject.ProjectId;
        GitInfoId = userProject.GitInfoId;
        Name = userProject.Project.Name;
        Slug = userProject.Project.Slug;
        Thumbnail = userProject.Project.AvatarUrl;
        Official = userProject.Project.Workspace.OwnerId is null;
    }

    /// <summary>
    /// The user project (session) id.
    /// </summary>
    [Required]
    public Guid Id { get; set; }

    [Required]
    public EntityObjectState State { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    public Guid GitInfoId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Slug { get; set; } = null!;

    [Required]
    public string? Thumbnail { get; set; }

    /// <summary>
    /// True when the project is owned by an organization (staff curated) rather than a user.
    /// </summary>
    [Required]
    public bool Official { get; set; }
}

/// <summary>
/// The rubric a review was conducted with. The full criteria live on the rubric endpoints.
/// </summary>
public class ReviewRubricDO
{
    public ReviewRubricDO() { }

    public ReviewRubricDO(Rubric rubric)
    {
        Id = rubric.Id;
        Name = rubric.Name;
        Slug = rubric.Slug;
    }

    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Slug { get; set; } = null!;
}

public class ReviewDO : BaseEntityDO<Review>
{
    public ReviewDO() { }

    public ReviewDO(Review review) : base(review)
    {
        Kind = review.Kind;
        State = review.State;
        RoundId = review.RoundId;
        Ref = review.Ref;
        Sha = review.Sha;
        Passed = review.Passed;
        ClaimedAt = review.ClaimedAt;
        StartedAt = review.StartedAt;
        FinishedAt = review.FinishedAt;
        UserProject = new(review.UserProject);
        Reviewer = review.Reviewer;
        Rubric = new(review.Rubric);
    }

    [Required]
    public ReviewKinds Kind { get; set; }

    [Required]
    public ReviewState State { get; set; }

    /// <summary>
    /// The evaluation round this review is a slot of. Null means it is advisory
    /// feedback that doesn't count towards completing the project.
    /// </summary>
    [Required]
    public Guid? RoundId { get; set; }

    /// <summary>
    /// The branch that was targeted when the review was created. It moves with every push, use <see cref="Sha"/> for what was reviewed.
    /// </summary>
    [Required]
    public string Ref { get; set; } = null!;

    /// <summary>
    /// The exact commit that was reviewed. Null for reviews created before SHAs were recorded.
    /// </summary>
    [Required]
    public string? Sha { get; set; }

    /// <summary>
    /// The reviewer's verdict ("is this project a pass?"), once the review is finished.
    /// </summary>
    [Required]
    public bool? Passed { get; set; }

    /// <summary>
    /// When the reviewer claimed this review. They then have 2 days to start it, and 24 hours to finish it once started.
    /// </summary>
    [Required]
    public DateTimeOffset? ClaimedAt { get; set; }

    /// <summary>
    /// When the reviewer started. A started review has 24 hours to be finished before it is released.
    /// </summary>
    [Required]
    public DateTimeOffset? StartedAt { get; set; }

    [Required]
    public DateTimeOffset? FinishedAt { get; set; }

    [Required]
    public ReviewProjectDO UserProject { get; set; } = null!;

    /// <summary>
    /// The user performing the review, if assigned.
    /// </summary>
    [Required]
    public UserBriefDO? Reviewer { get; set; }

    [Required]
    public ReviewRubricDO Rubric { get; set; } = null!;

    public static implicit operator ReviewDO?(Review? review) =>
        review is null ? null : new(review);
}
