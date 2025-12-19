// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Reviews;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Reviews;

/// <summary>
/// Data object representing a rubric for reviews.
/// </summary>
public class RubricDO(Rubric rubric) : BaseEntityDO<Rubric>(rubric)
{
    [Required]
    public string Name { get; set; } = rubric.Name;

    [Required]
    public string Markdown { get; set; } = rubric.Markdown;

    [Required]
    public bool Public { get; set; } = rubric.Public;

    [Required]
    public bool Enabled { get; set; } = rubric.Enabled;

    [Required]
    public Guid ProjectId { get; set; } = rubric.ProjectId;

    [Required]
    public Guid CreatorId { get; set; } = rubric.CreatorId;

    [Required]
    public Guid GitInfoId { get; set; } = rubric.GitInfoId;

    /// <summary>
    /// The project this rubric belongs to.
    /// </summary>
    public Projects.ProjectDO? Project { get; set; } = rubric.Project;

    /// <summary>
    /// The user who created this rubric.
    /// </summary>
    public UserLightDO? Creator { get; set; } = rubric.Creator;

    /// <summary>
    /// Git information for this rubric.
    /// </summary>
    public GitDO? GitInfo { get; set; } = rubric.GitInfo;

    public static implicit operator RubricDO?(Rubric? rubric) =>
        rubric is null ? null : new(rubric);
}
