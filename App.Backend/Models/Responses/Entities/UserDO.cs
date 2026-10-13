// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using App.Backend.Domain.Entities.Users;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

/// <summary>
/// A detailed data object representing a user.
/// </summary>
public class UserDO : UserLightDO
{
    public UserDO() { }

    public UserDO(User user) : base(user)
    {
        Details = user.Details;
    }

    public UserDetailsDO? Details { get; set; }

    public static implicit operator UserDO?(User? user) => user is null ? null : new(user);
}
