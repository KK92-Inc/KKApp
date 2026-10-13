// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Users;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

/// <summary>
/// The bare minimum needed to render "who is this": name and avatar.
/// Use this when a user is only referenced by another object. No email, role or timestamps.
/// </summary>
public class UserBriefDO
{
    public UserBriefDO() { }

    public UserBriefDO(User user)
    {
        Id = user.Id;
        Login = user.Login;
        DisplayName = user.Display;
        AvatarUrl = user.AvatarUrl;
    }

    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Login { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string? AvatarUrl { get; set; }

    public static implicit operator UserBriefDO?(User? user) => user is null ? null : new(user);
}
