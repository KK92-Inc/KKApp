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
public class MemberDO : BaseEntityDO<Member>
{
    public MemberDO() { }

    public MemberDO(Member member) : base(member)
    {
        EntityId = member.EntityId;
        EntityType = member.EntityType;
        UserId = member.UserId;
        Role = member.Role;
        LeftAt = member.LeftAt;
        User = member.User;
    }

    [Required]
    public Guid EntityId { get; set; }

    [Required]
    public MemberEntityType EntityType { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public MemberRole Role { get; set; }

    /// <summary>
    /// When the member left the project, if applicable.
    /// </summary>
    [Required]
    public DateTimeOffset? LeftAt { get; set; }

    /// <summary>
    /// The user who is a member of the project.
    /// </summary>
    [Required]
    public UserLightDO User { get; set; } = null!;

    public static implicit operator MemberDO?(Member? member) =>
        member is null ? null : new(member);
}
