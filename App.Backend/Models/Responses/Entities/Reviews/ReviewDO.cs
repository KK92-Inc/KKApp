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
public class ReviewProjectDO(UserProject userProject)
{
    /// <summary>
    /// The user project (session) id.
    /// </summary>
    [Required]
    public Guid Id { get; init; } = userProject.Id;

    [Required]
    public EntityObjectState State { get; init; } = userProject.State;

    [Required]
    public Guid ProjectId { get; init; } = userProject.ProjectId;

    [Required]
    public string Name { get; init; } = userProject.Project.Name;

    [Required]
    public string Slug { get; init; } = userProject.Project.Slug;

    [Required]
    public string? Thumbnail { get; init; } = userProject.Project.AvatarUrl;

    /// <summary>
    /// True when the project is owned by an organization (staff curated) rather than a user.
    /// </summary>
    [Required]
    public bool Official { get; init; } = userProject.Project.Workspace.Ownership is EntityOwnership.Organization;
}

/// <summary>
/// The rubric a review was conducted with. The full criteria live on the rubric endpoints.
/// </summary>
public class ReviewRubricDO(Rubric rubric)
{
    [Required]
    public Guid Id { get; init; } = rubric.Id;

    [Required]
    public string Name { get; init; } = rubric.Name;

    [Required]
    public string Slug { get; init; } = rubric.Slug;
}

public class ReviewDO(Review review) : BaseEntityDO<Review>(review)
{
    [Required]
    public ReviewKinds Kind { get; init; } = review.Kind;

    [Required]
    public ReviewState State { get; init; } = review.State;

    /// <summary>
    /// The evaluation round this review is a slot of. Null means it is advisory
    /// feedback that doesn't count towards completing the project.
    /// </summary>
    [Required]
    public Guid? RoundId { get; init; } = review.RoundId;

    /// <summary>
    /// The branch that was targeted when the review was created. It moves with every push, use <see cref="Sha"/> for what was reviewed.
    /// </summary>
    [Required]
    public string Ref { get; init; } = review.Ref;

    /// <summary>
    /// The exact commit that was reviewed. Null for reviews created before SHAs were recorded.
    /// </summary>
    [Required]
    public string? Sha { get; init; } = review.Sha;

    /// <summary>
    /// The reviewer's verdict ("is this project a pass?"), once the review is finished.
    /// </summary>
    [Required]
    public bool? Passed { get; init; } = review.Passed;

    /// <summary>
    /// When the reviewer claimed this review. They then have 2 days to start it, and 24 hours to finish it once started.
    /// </summary>
    [Required]
    public DateTimeOffset? ClaimedAt { get; init; } = review.ClaimedAt;

    /// <summary>
    /// When the reviewer started. A started review has 24 hours to be finished before it is released.
    /// </summary>
    [Required]
    public DateTimeOffset? StartedAt { get; init; } = review.StartedAt;

    [Required]
    public DateTimeOffset? FinishedAt { get; init; } = review.FinishedAt;

    [Required]
    public ReviewProjectDO UserProject { get; init; } = new(review.UserProject);

    /// <summary>
    /// The user performing the review, if assigned.
    /// </summary>
    [Required]
    public UserBriefDO? Reviewer { get; init; } = review.Reviewer;

    [Required]
    public ReviewRubricDO Rubric { get; init; } = new(review.Rubric);

    public static implicit operator ReviewDO?(Review? review) =>
        review is null ? null : new(review);
}
