// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursi;

public class UserCursusDO : BaseEntityDO<UserCursus>
{
    public UserCursusDO() { }

    public UserCursusDO(UserCursus userCursus) : base(userCursus)
    {
        User = userCursus.User;
        Cursus = userCursus.Cursus;
        State = userCursus.State;
        UnlocksAt = userCursus.UnlocksAt;
    }

    [Required]
    public UserBriefDO User { get; set; } = null!;
    
    [Required]
    public CursusDO Cursus { get; set; } = null!;
    
    [Required]
    public EntityObjectState State { get; set; }
    
    [Required]
    public DateTimeOffset? UnlocksAt { get; set; }

    public static implicit operator UserCursusDO?(UserCursus? userCursus) => userCursus is null ? null : new(userCursus);
}
