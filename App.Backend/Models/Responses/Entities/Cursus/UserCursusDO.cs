// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursus;

public class UserCursusDO(UserCursus userCursus) : BaseEntityDO<UserCursus>(userCursus)
{
    [Required]
    public EntityObjectState State { get; set; } = userCursus.State;
    
    [Required]
    public DateTimeOffset? UnlocksAt { get; set; } = userCursus.UnlocksAt;
    
    [Required]
    public CursusDO Cursus { get; set; } = userCursus.Cursus;

    [Required]
    public UserLightDO? User { get; set; } = userCursus.User;

    public static implicit operator UserCursusDO?(UserCursus? userCursus) =>
        userCursus is null ? null : new(userCursus);
}
