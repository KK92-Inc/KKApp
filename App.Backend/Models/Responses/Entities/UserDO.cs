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
public class UserDO : BaseEntityDO<User>
{
    public UserDO() { } // REQUIRED for deserialization

    public UserDO(User user) : base(user)
    {
        Login = user.Login;
        DisplayName = user.Display;
        AvatarUrl = user.AvatarUrl;
        Details = user.Details;
    }

    [Required]
    public string Login { get; set; } = default!;

    public string? DisplayName { get; set; }

    public string? AvatarUrl { get; set; }

    public UserDetailsDO? Details { get; set; }

    public static implicit operator UserDO?(User? user) => user is null ? null : new(user);
}
