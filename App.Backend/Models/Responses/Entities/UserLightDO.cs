// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using App.Backend.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

/// <summary>
/// A lightweight version of <see cref="User"/> containing only minimal user information.
/// </summary>
/// <param name="user"></param>
public class UserLightDO(User user) : BaseEntityDO<User>(user)
{
    [Required]
    public string Login { get; set; } = user.Login;

    [Required]
    public string? DisplayName { get; set; } = user.Display;

    [Required]
    public string? AvatarUrl { get; set; } = user.AvatarUrl;

    /// <summary>
    /// Utility operator to let us just pass the user entity into the constructor
    /// </summary>
    /// <param name="user">The entity</param>
    public static implicit operator UserLightDO?(User? user) => user is null ? null : new(user);
}
