// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
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
        ProjectGitInfoId = userProject.Project.GitId;
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
    public Guid ProjectGitInfoId { get; set; }

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
