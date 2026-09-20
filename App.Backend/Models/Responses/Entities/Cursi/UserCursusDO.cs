// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursi;

public class UserCursusDO(UserCursus userCursus) : BaseEntityDO<UserCursus>(userCursus)
{
    public Guid UserId { get; set; } = userCursus.UserId;
    public Guid CursusId { get; set; } = userCursus.CursusId;
    public EntityObjectState State { get; set; } = userCursus.State;
    public DateTimeOffset? UnlocksAt { get; set; } = userCursus.UnlocksAt;

    public static implicit operator UserCursusDO?(UserCursus? userCursus) => userCursus is null ? null : new(userCursus);
}
