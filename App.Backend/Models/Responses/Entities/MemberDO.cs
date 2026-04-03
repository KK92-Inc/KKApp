// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Projects;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Projects;

/// <summary>
/// Data object representing a member of a user project.
/// </summary>
public class MemberDO(Member member) : BaseEntityDO<Member>(member)
{
    [Required]
    public Guid EntityId { get; set; } = member.EntityId;

    [Required]
    public MemberEntityType EntityType { get; set; } = member.EntityType;

    [Required]
    public Guid UserId { get; set; } = member.UserId;

    [Required]
    public MemberRole Role { get; set; } = member.Role;

    /// <summary>
    /// When the member left the project, if applicable.
    /// </summary>
    [Required]
    public DateTimeOffset? LeftAt { get; set; } = member.LeftAt;

    /// <summary>
    /// The user who is a member of the project.
    /// </summary>
    [Required]
    public UserLightDO User { get; set; } = member.User;

    public static implicit operator MemberDO?(Member? member) =>
        member is null ? null : new(member);
}
