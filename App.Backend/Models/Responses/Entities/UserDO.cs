// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using App.Backend.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

/// <summary>
/// A detailed data object representing a user.
/// </summary>
/// <param name="user"></param>
public class UserDO(User user) : BaseEntityDO<User>(user)
{
    [Required]
    public string Login { get; set; } = user.Login;

    [Required]
    public string? DisplayName { get; set; } = user.Display;

    [Required]
    public string? AvatarUrl { get; set; } = user.AvatarUrl;

    [Required]
    public virtual UserDetailsDO? Details { get; set; } = user.Details;

    /// <summary>
    /// Utility operator to let us just pass the user entity into the constructor
    /// </summary>
    /// <param name="user">The entity</param>
    public static implicit operator UserDO?(User? user) => user is null ? null : new(user);
}
