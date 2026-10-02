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
public class UserBriefDO(User user)
{
    [Required]
    public Guid Id { get; init; } = user.Id;

    [Required]
    public string Login { get; init; } = user.Login;

    public string? DisplayName { get; init; } = user.Display;

    public string? AvatarUrl { get; init; } = user.AvatarUrl;

    public static implicit operator UserBriefDO?(User? user) => user is null ? null : new(user);
}
