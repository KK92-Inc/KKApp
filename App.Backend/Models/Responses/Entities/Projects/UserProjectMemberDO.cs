// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Projects;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Projects;

/// <summary>
/// Data object representing a member of a user project.
/// </summary>
public class UserProjectMemberDO(UserProjectMember member) : BaseEntityDO<UserProjectMember>(member)
{
    [Required]
    public Guid UserProjectId { get; set; } = member.UserProjectId;

    [Required]
    public Guid UserId { get; set; } = member.UserId;

    [Required]
    public UserProjectRole Role { get; set; } = member.Role;

    /// <summary>
    /// When the member left the project, if applicable.
    /// </summary>
    public DateTimeOffset? LeftAt { get; set; } = member.LeftAt;

    /// <summary>
    /// The user who is a member of the project.
    /// </summary>
    public UserLightDO? User { get; set; } = member.User;

    public static implicit operator UserProjectMemberDO?(UserProjectMember? member) =>
        member is null ? null : new(member);
}
